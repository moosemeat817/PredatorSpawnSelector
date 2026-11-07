using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Text;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using MelonLoader.Utils;

namespace PredatorSpawnSelector;

// TEMPORARY DIAGNOSTIC - remove once the spawn-list rewrite is done.
//
// This version uses REFLECTION for everything related to the new spawn API (m_SpawnList, SpawnListEntry,
// AssetReferenceBaseAi, PickSpawnAnimal), so it compiles no matter which assemblies the project references,
// and reports what actually exists in the game's assemblies at RUNTIME.
//
// What it does:
//   1. Once per session, dumps the runtime interop surface of SpawnRegion (ctors/properties/methods/nested types),
//      plus its primitive property values for the first region seen.
//   2. If SpawnRegion has m_SpawnList: dumps SpawnListEntry and AssetReferenceBaseAi types, every entry's
//      primitive properties (weights etc.), and the AssetGUID + loaded asset name of m_Normal / m_Aurora.
//   3. Emits "GUID-TABLE" lines: unique GUID -> animal name.
//   4. If PickSpawnAnimal exists, calls it 20 times per region and tallies results (shows weighting).
//      If that has side effects, delete the SamplePicks(region) call.
//
// Output: MelonLoader log (prefix [PSS-DIAG]) AND UserData/PSS_SpawnListDiagnostic.txt.
internal static class SpawnListDiagnostic
{
    // Master switch: the "Enable Diagnostic Logging" option in the mod settings. Everything below is skipped when it is off.
    internal static bool Enabled => Settings.instance.enableDiagnostics;

    private const BindingFlags Declared =
        BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
    private const BindingFlags AnyInstance =
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

    private static readonly HashSet<int> _seenRegions = new HashSet<int>();
    private static readonly HashSet<string> _seenGuids = new HashSet<string>();
    private static bool _spawnRegionDumped;
    private static bool _entryTypeDumped;
    private static bool _refTypeDumped;
    private static bool _listTypeDumped;
    private static bool _catalogScanned;
    private static string _path;

    private static string FilePath
    {
        get
        {
            if (_path == null)
            {
                _path = Path.Combine(MelonEnvironment.UserDataDirectory, "PSS_SpawnListDiagnostic.txt");
            }
            return _path;
        }
    }

    internal static void Log(string msg)
    {
        if (!Enabled)
        {
            return;
        }

        MelonLogger.Msg("[PSS-DIAG] " + msg);
        try
        {
            File.AppendAllText(FilePath, DateTime.Now.ToString("HH:mm:ss.fff") + " " + msg + Environment.NewLine);
        }
        catch
        {
            // File logging is best-effort; the MelonLoader log still has everything.
        }
    }

    // ---------- reflection helpers ----------

    private static object GetValue(object obj, string name)
    {
        Type t = obj.GetType();
        PropertyInfo p = t.GetProperty(name, AnyInstance);
        if (p != null && p.GetIndexParameters().Length == 0)
        {
            return p.GetValue(obj);
        }
        FieldInfo f = t.GetField(name, AnyInstance);
        if (f != null)
        {
            return f.GetValue(obj);
        }
        throw new MissingMemberException(t.Name, name);
    }

    private static string Str(object obj, string name)
    {
        try
        {
            return GetValue(obj, name)?.ToString() ?? "null";
        }
        catch (Exception e)
        {
            return "MISSING(" + (e.InnerException ?? e).GetType().Name + ")";
        }
    }

    private static MethodInfo FindMethod(Type t, string name, int paramCount)
    {
        foreach (MethodInfo m in t.GetMethods(AnyInstance))
        {
            if (m.Name == name && m.GetParameters().Length == paramCount)
            {
                return m;
            }
        }
        return null;
    }

    private static string AssetName(object reference)
    {
        if (reference == null)
        {
            return "null";
        }
        MethodInfo m = FindMethod(reference.GetType(), "GetOrLoadTypedAsset", 0);
        if (m == null)
        {
            return "NO_GetOrLoadTypedAsset";
        }
        object asset = m.Invoke(reference, null);
        if (asset == null)
        {
            return "null";
        }
        if (asset is UnityEngine.Component c)
        {
            return c.gameObject.name;
        }
        if (asset is UnityEngine.Object o)
        {
            return o.name;
        }
        return asset.ToString() ?? "";
    }


    // ---------- addressables catalog scan ----------
    // Finds GUIDs for animals that are not on any SpawnRegion in the current scene (starving wolves, challenge bears).
    private static bool IsGuidKey(string key)
    {
        if (key == null || key.Length != 32)
        {
            return false;
        }
        foreach (char c in key)
        {
            bool hex = (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
            if (!hex)
            {
                return false;
            }
        }
        return true;
    }

    private static bool LooksInteresting(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }
        return text.IndexOf("WILDLIFE_", StringComparison.OrdinalIgnoreCase) >= 0
            || text.IndexOf("starving", StringComparison.OrdinalIgnoreCase) >= 0
            || text.IndexOf("BearHunted", StringComparison.OrdinalIgnoreCase) >= 0
            || text.IndexOf("Challenge", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    // Reads the Addressables catalog FILE from disk instead of walking it through Il2Cpp (that crashed the game).
    // Pure managed code, so it cannot take the game down. Maps GUID keys -> address / internal id and logs animal-ish ones.
    // Output lines start with "CATALOG".
    private const bool EnableCatalogFileScan = false;

    private static IEnumerator DelayedCatalogScan()
    {
        yield return new UnityEngine.WaitForSeconds(5f);
        ScanCatalogFile();
    }

    private static void ScanCatalogFile()
    {
        if (!Enabled || _catalogScanned || !EnableCatalogFileScan)
        {
            return;
        }
        _catalogScanned = true;

        try
        {
            string root = MelonEnvironment.GameRootDirectory;
            Log($"CATALOG begin | gameRoot={root}");

            var files = new List<string>();
            foreach (string dataDir in Directory.GetDirectories(root, "*_Data"))
            {
                string streaming = Path.Combine(dataDir, "StreamingAssets");
                if (!Directory.Exists(streaming))
                {
                    continue;
                }
                string aa = Path.Combine(streaming, "aa");
                string searchIn = Directory.Exists(aa) ? aa : streaming;
                files.AddRange(Directory.GetFiles(searchIn, "catalog*.*", SearchOption.AllDirectories));
            }

            Log($"CATALOG files found: {files.Count}");
            foreach (string f in files)
            {
                Log($"CATALOG file | {f} | {new FileInfo(f).Length} bytes");
            }

            foreach (string f in files)
            {
                string ext = Path.GetExtension(f).ToLowerInvariant();
                if (ext == ".json")
                {
                    ParseJsonCatalog(f);
                }
                else if (ext == ".bin")
                {
                    ScanBinaryCatalogNames(f);
                }
            }

            Log("CATALOG scan done");
        }
        catch (Exception ex)
        {
            Log($"CATALOG scan failed: {ex}");
        }
    }

    private static string ReadCatalogKey(byte[] keyData, int offset)
    {
        if (offset < 0 || offset >= keyData.Length)
        {
            return null;
        }
        byte type = keyData[offset];
        offset++;
        if (type == 0) // ascii string
        {
            int len = BitConverter.ToInt32(keyData, offset);
            return System.Text.Encoding.ASCII.GetString(keyData, offset + 4, len);
        }
        if (type == 1) // unicode string (length in bytes)
        {
            int len = BitConverter.ToInt32(keyData, offset);
            return System.Text.Encoding.Unicode.GetString(keyData, offset + 4, len);
        }
        return null; // numeric / hash / type / json keys are not interesting here
    }

    private static void ParseJsonCatalog(string path)
    {
        Log($"CATALOG parsing JSON catalog {Path.GetFileName(path)}");

        using (System.Text.Json.JsonDocument doc = System.Text.Json.JsonDocument.Parse(File.ReadAllBytes(path)))
        {
            System.Text.Json.JsonElement rootEl = doc.RootElement;

            byte[] keyData = Convert.FromBase64String(rootEl.GetProperty("m_KeyDataString").GetString());
            byte[] bucketData = Convert.FromBase64String(rootEl.GetProperty("m_BucketDataString").GetString());
            byte[] entryData = Convert.FromBase64String(rootEl.GetProperty("m_EntryDataString").GetString());

            var internalIds = new List<string>();
            foreach (System.Text.Json.JsonElement e in rootEl.GetProperty("m_InternalIds").EnumerateArray())
            {
                internalIds.Add(e.GetString());
            }

            int bucketCount = BitConverter.ToInt32(bucketData, 0);
            var keys = new string[bucketCount];
            var bucketEntries = new int[bucketCount][];
            int off = 4;
            for (int i = 0; i < bucketCount; i++)
            {
                int keyOffset = BitConverter.ToInt32(bucketData, off);
                off += 4;
                int n = BitConverter.ToInt32(bucketData, off);
                off += 4;
                var entries = new int[n];
                for (int c = 0; c < n; c++)
                {
                    entries[c] = BitConverter.ToInt32(bucketData, off);
                    off += 4;
                }
                bucketEntries[i] = entries;
                keys[i] = ReadCatalogKey(keyData, keyOffset);
            }

            int entryCount = BitConverter.ToInt32(entryData, 0);
            Log($"CATALOG json | buckets={bucketCount} entries={entryCount} internalIds={internalIds.Count}");

            int guidKeys = 0;
            int hits = 0;
            for (int i = 0; i < bucketCount; i++)
            {
                string key = keys[i];
                if (key == null)
                {
                    continue;
                }
                bool isGuid = IsGuidKey(key);
                if (isGuid)
                {
                    guidKeys++;
                }

                foreach (int entryIndex in bucketEntries[i])
                {
                    int baseOff = 4 + entryIndex * 28; // 7 ints per entry
                    if (baseOff + 28 > entryData.Length)
                    {
                        continue;
                    }
                    int internalIdIdx = BitConverter.ToInt32(entryData, baseOff);
                    int primaryKeyBucket = BitConverter.ToInt32(entryData, baseOff + 20);
                    string address = primaryKeyBucket >= 0 && primaryKeyBucket < bucketCount ? keys[primaryKeyBucket] : null;
                    string internalId = internalIdIdx >= 0 && internalIdIdx < internalIds.Count ? internalIds[internalIdIdx] : null;

                    if (LooksInteresting(address) || LooksInteresting(internalId) || (!isGuid && LooksInteresting(key)))
                    {
                        hits++;
                        Log($"{(isGuid ? "CATALOG-GUID" : "CATALOG-KEY")} | key={key} | address={address} | internalId={internalId}");
                        break;
                    }
                }
            }

            Log($"CATALOG json done | guidKeys={guidKeys} animal-ish hits={hits}");
        }
    }

    // Binary catalogs (catalog.bin) are not decoded; just list the animal-looking names so we at least see what exists.
    private static void ScanBinaryCatalogNames(string path)
    {
        Log($"CATALOG binary catalog {Path.GetFileName(path)}: listing names only (GUID mapping not decoded)");
        string text = System.Text.Encoding.Latin1.GetString(File.ReadAllBytes(path));
        var seen = new HashSet<string>();
        foreach (Match m in Regex.Matches(text, @"[A-Za-z0-9_./\-]*(WILDLIFE_|BearHunted|Wolf_starving)[A-Za-z0-9_./\-]*"))
        {
            if (seen.Add(m.Value) && seen.Count <= 300)
            {
                Log($"CATALOG-NAME | {m.Value}");
            }
        }
    }

    // ---------- main inspection ----------

    internal static void Inspect(SpawnRegion region)
    {
        if (!Enabled)
        {
            return;
        }

        try
        {
            if (region == null || !_seenRegions.Add(region.GetInstanceID()))
            {
                return;
            }

            if (!_spawnRegionDumped)
            {
                _spawnRegionDumped = true;
                Log($"##### Diagnostic session {DateTime.Now:yyyy-MM-dd HH:mm:ss} #####");
                DumpType(region.GetType());
                Log($"REGION-VALUES (first region seen) | {DumpValues(region)}");
            }

            if (EnableCatalogFileScan)
            {
                MelonCoroutines.Start(DelayedCatalogScan());
            }

            string scene = GameManager.m_ActiveScene;
            string regionName = region.gameObject.name;

            // Only wolves and bears matter for this mod; skip stags, rabbits, moose, ptarmigan, etc.
            string subtype = Str(region, "m_AiSubTypeSpawned");
            if (subtype != "Wolf" && subtype != "Bear")
            {
                return;
            }
            string header = $"scene={scene} | name={regionName} | subtype={Str(region, "m_AiSubTypeSpawned")} | mode={Str(region, "m_WildlifeMode")} | active={region.gameObject.activeSelf} | radius={Str(region, "m_Radius")} | chanceActive={Str(region, "m_ChanceActive")}";

            object listObj;
            try
            {
                listObj = GetValue(region, "m_SpawnList");
            }
            catch (Exception)
            {
                Log($"REGION | {header} | m_SpawnList=MISSING on runtime type {region.GetType().FullName}");
                return;
            }

            if (listObj == null)
            {
                Log($"REGION | {header} | m_SpawnList=NULL");
                return;
            }

            List<object> entries = ReadList(listObj);
            if (entries == null)
            {
                Log($"REGION | {header} | could not read m_SpawnList of type {listObj.GetType().FullName} (no IEnumerable, no Count/Item)");
                if (!_listTypeDumped)
                {
                    _listTypeDumped = true;
                    DumpType(listObj.GetType());
                }
                return;
            }

            Log($"REGION | {header} | entries={entries.Count}");

            string context = $"{scene}/{regionName}";
            for (int i = 0; i < entries.Count; i++)
            {
                object entry = entries[i];
                if (entry == null)
                {
                    Log($"  ENTRY[{i}] NULL");
                    continue;
                }

                if (!_entryTypeDumped)
                {
                    _entryTypeDumped = true;
                    DumpType(entry.GetType());
                }

                Log($"  ENTRY[{i}] | {DumpValues(entry)}");

                foreach (string slotName in new[] { "m_Normal", "m_Aurora" })
                {
                    object reference = null;
                    string refError = "";
                    try
                    {
                        reference = GetValue(entry, slotName);
                    }
                    catch (Exception ex)
                    {
                        refError = "MISSING(" + (ex.InnerException ?? ex).GetType().Name + ")";
                    }

                    if (reference != null && !_refTypeDumped)
                    {
                        _refTypeDumped = true;
                        DumpType(reference.GetType());
                    }

                    Slot(slotName, reference, refError, context);
                }
            }

            SamplePicks(region);
        }
        catch (Exception ex)
        {
            Log($"INSPECT ERROR on {region?.gameObject?.name}: {ex}");
        }
    }

    // Il2Cpp lists don't implement System.Collections.IEnumerable, so fall back to Count + Item[int] via reflection.
    private static List<object> ReadList(object listObj)
    {
        var result = new List<object>();

        if (listObj is IEnumerable managed)
        {
            foreach (object e in managed)
            {
                result.Add(e);
            }
            return result;
        }

        Type t = listObj.GetType();
        PropertyInfo countProp = t.GetProperty("Count", AnyInstance);
        PropertyInfo itemProp = null;
        foreach (PropertyInfo p in t.GetProperties(AnyInstance))
        {
            if (p.Name == "Item" && p.GetIndexParameters().Length == 1)
            {
                itemProp = p;
                break;
            }
        }

        if (countProp == null || itemProp == null)
        {
            return null;
        }

        int count = Convert.ToInt32(countProp.GetValue(listObj));
        for (int i = 0; i < count; i++)
        {
            result.Add(itemProp.GetValue(listObj, new object[] { i }));
        }
        return result;
    }

    private static void Slot(string label, object reference, string refError, string context)
    {
        if (refError != "")
        {
            Log($"    SLOT {label} | {refError}");
            return;
        }

        string guid = reference == null ? "null" : Str(reference, "AssetGUID");
        string name;
        try
        {
            name = AssetName(reference);
        }
        catch (Exception e)
        {
            name = "ERR:" + (e.InnerException ?? e).Message;
        }

        Log($"    SLOT {label} | guid={guid} | asset={name}");

        if (guid != "null" && !guid.StartsWith("MISSING") && !string.IsNullOrEmpty(guid) && _seenGuids.Add(guid))
        {
            Log($"GUID-TABLE | {name} | {guid} | slot={label} | first seen: {context}");
        }
    }

    private static void SamplePicks(SpawnRegion region)
    {
        try
        {
            MethodInfo pick = FindMethod(region.GetType(), "PickSpawnAnimal", 1);
            if (pick == null)
            {
                Log("  PICKS | PickSpawnAnimal(1 param) not found on runtime type");
                return;
            }

            object mode = GetValue(region, "m_WildlifeMode");
            var tally = new Dictionary<string, int>();
            for (int i = 0; i < 20; i++)
            {
                object picked = pick.Invoke(region, new[] { mode });
                string key = AssetName(picked);
                tally[key] = tally.TryGetValue(key, out int c) ? c + 1 : 1;
            }

            var sb = new StringBuilder();
            foreach (var kv in tally)
            {
                sb.Append($"{kv.Key} x{kv.Value}; ");
            }
            Log($"  PICKS(20) | {sb}");
        }
        catch (Exception e)
        {
            Log($"  PICKS failed: {(e.InnerException ?? e).Message}");
        }
    }

    // ---------- post-change report ----------
    // Runs AFTER the mod's own postfixes (Priority.Last) on SpawnRegion.Deserialize and SpawnRegion.Start.
    // Shows what the region's spawn list looks like after PSS changed it, what the game's own picker now returns,
    // and which animals already exist / are queued from the save (those are NOT affected by a spawn-list change).
    private const bool EnablePostChangeReport = true;
    private static readonly HashSet<string> _dumpedElementTypes = new HashSet<string>();
    private static int _serializedLogged;

    internal static string FriendlyName(string guid)
    {
        if (guid == AnimalRefs.Wolf) return "Wolf";
        if (guid == AnimalRefs.WolfAurora) return "Wolf_aurora";
        if (guid == AnimalRefs.Timberwolf) return "Timberwolf";
        if (guid == AnimalRefs.TimberwolfAurora) return "Timberwolf_aurora";
        if (guid == AnimalRefs.WolfStarving) return "Starving";
        if (guid == AnimalRefs.WolfStarvingAurora) return "Starving_aurora";
        if (guid == AnimalRefs.Bear) return "Bear";
        if (guid == AnimalRefs.BearAurora) return "Bear_aurora";
        if (guid == AnimalRefs.BearChallenge) return "BearChallenge";
        return guid;
    }

    private static string GuidOf(object owner, string slot)
    {
        try
        {
            object reference = GetValue(owner, slot);
            return reference == null ? "null" : Str(reference, "AssetGUID");
        }
        catch (Exception)
        {
            return "?";
        }
    }

    private static string DescribeElement(object e)
    {
        if (e == null)
        {
            return "null";
        }
        if (e is UnityEngine.Component c)
        {
            return c.gameObject.name;
        }
        if (e is UnityEngine.Object o)
        {
            return o.name;
        }
        string typeName = e.GetType().FullName;
        if (_dumpedElementTypes.Add(typeName))
        {
            DumpType(e.GetType());
        }
        return "{" + DumpValues(e) + "}";
    }

    private static string DescribeCollection(object owner, string field)
    {
        try
        {
            object coll = GetValue(owner, field);
            if (coll == null)
            {
                return field + "=null";
            }
            List<object> items = ReadList(coll);
            if (items == null)
            {
                return field + " (" + coll.GetType().Name + ") count=" + Str(coll, "Count") + " [not readable as list]";
            }
            var sb = new StringBuilder();
            sb.Append(field).Append(" count=").Append(items.Count);
            for (int i = 0; i < items.Count && i < 3; i++)
            {
                sb.Append(" | [").Append(i).Append("] ").Append(DescribeElement(items[i]));
            }
            return sb.ToString();
        }
        catch (Exception ex)
        {
            return field + " ERR " + (ex.InnerException ?? ex).GetType().Name;
        }
    }

    internal static void PostChangeReport(SpawnRegion region, string phase)
    {
        if (!Enabled || !EnablePostChangeReport)
        {
            return;
        }

        try
        {
            if (region == null || Str(region, "m_AiSubTypeSpawned") != "Wolf")
            {
                return;
            }

            var listText = new StringBuilder();
            object listObj = GetValue(region, "m_SpawnList");
            List<object> entries = listObj == null ? null : ReadList(listObj);
            if (entries != null)
            {
                foreach (object entry in entries)
                {
                    if (entry == null)
                    {
                        continue;
                    }
                    listText.Append(FriendlyName(GuidOf(entry, "m_Normal"))).Append('/')
                            .Append(FriendlyName(GuidOf(entry, "m_Aurora"))).Append(" w=")
                            .Append(Str(entry, "m_Weight")).Append("; ");
                }
            }

            string picksText = "n/a";
            MethodInfo pick = FindMethod(region.GetType(), "PickSpawnAnimal", 1);
            if (pick != null)
            {
                object mode = GetValue(region, "m_WildlifeMode");
                var tally = new Dictionary<string, int>();
                for (int i = 0; i < 20; i++)
                {
                    object picked = pick.Invoke(region, new[] { mode });
                    string key = picked == null ? "null" : FriendlyName(Str(picked, "AssetGUID"));
                    tally[key] = tally.TryGetValue(key, out int c) ? c + 1 : 1;
                }
                var pt = new StringBuilder();
                foreach (var kv in tally)
                {
                    pt.Append(kv.Key).Append(" x").Append(kv.Value).Append("; ");
                }
                picksText = pt.ToString();
            }

            string flags = $"wasEnabled={Str(region, "m_WasEnabled")} wasForceDisabled={Str(region, "m_WasForceDisabled")} onlyOneChance={Str(region, "m_OnlyOneChance")} randomSpawner={Str(region, "m_ControlledByRandomSpawner")} registered={Str(region, "m_Registered")} chance={Str(region, "m_ChanceActive")}";
            Log($"POST[{phase}] | scene={GameManager.m_ActiveScene} | region={region.gameObject.name} | active={region.gameObject.activeSelf} | {flags} | list=[{listText}] | picks=[{picksText}]");

            // The region's own save string: shows what the game stores about this region (capped to a few samples).
            if (_serializedLogged < 8)
            {
                MethodInfo ser = FindMethod(region.GetType(), "Serialize", 0);
                if (ser != null)
                {
                    _serializedLogged++;
                    string text = ser.Invoke(region, null)?.ToString() ?? "null";
                    Log($"SERIALIZED[{phase}] | region={region.gameObject.name} | {(text.Length > 1500 ? text.Substring(0, 1500) + "..." : text)}");
                }
            }
            Log("    " + DescribeCollection(region, "m_Spawns"));
            Log("    " + DescribeCollection(region, "m_DeferredSpawnsWithSavedPosition"));
            Log("    " + DescribeCollection(region, "m_DeferredSpawnsWithRandomPosition"));
            try
            {
                object queue = GetValue(region, "m_PendingSerializedRespawnInfoQueue");
                Log("    m_PendingSerializedRespawnInfoQueue count=" + (queue == null ? "null" : Str(queue, "Count")));
            }
            catch (Exception)
            {
                Log("    m_PendingSerializedRespawnInfoQueue unreadable");
            }
        }
        catch (Exception ex)
        {
            Log($"POST[{phase}] ERROR: {ex}");
        }
    }

    // Called whenever the game rebuilds an animal from saved data. Shows what the save says and what actually came out.
    private static int _saveInstantiateLogged;

    internal static void LogSaveInstantiate(SpawnRegion region, object spawnData, object result)
    {
        try
        {
            if (!Enabled || region == null || Str(region, "m_AiSubTypeSpawned") != "Wolf" || _saveInstantiateLogged >= 40)
            {
                return;
            }
            _saveInstantiateLogged++;

            if (spawnData != null && _dumpedElementTypes.Add(spawnData.GetType().FullName))
            {
                DumpType(spawnData.GetType());
            }

            string resultName = result is UnityEngine.Component c ? c.gameObject.name : (result == null ? "null" : result.ToString());
            string data = spawnData == null ? "null" : DumpValues(spawnData);
            Log($"SAVE-INSTANTIATE | scene={GameManager.m_ActiveScene} | region={region.gameObject.name} | result={resultName} | saved={{{data}}}");
        }
        catch (Exception ex)
        {
            Log($"SAVE-INSTANTIATE ERROR: {ex.Message}");
        }
    }

    // Shows what a queued (pending) respawn from the save contains.
    private static int _pendingLogged;

    internal static void LogPendingRespawn(SpawnRegion region, object pending, object result)
    {
        try
        {
            if (!Enabled || region == null || Str(region, "m_AiSubTypeSpawned") != "Wolf" || _pendingLogged >= 10)
            {
                return;
            }
            _pendingLogged++;

            if (pending != null && _dumpedElementTypes.Add(pending.GetType().FullName))
            {
                DumpType(pending.GetType());
            }

            string resultName = result is UnityEngine.Component c ? c.gameObject.name : (result == null ? "null" : result.ToString());
            Log($"PENDING-RESPAWN | scene={GameManager.m_ActiveScene} | region={region.gameObject.name} | result={resultName} | pending={{{(pending == null ? "null" : DumpValues(pending))}}}");
        }
        catch (Exception ex)
        {
            Log($"PENDING-RESPAWN ERROR: {ex.Message}");
        }
    }

    // Logs what the game is told to restore for a region (the saved text) and the region's on/off state around the game's own Deserialize.
    private static int _deserializeLogged;

    internal static bool ShouldLogDeserialize(SpawnRegion region)
    {
        if (!Enabled || region == null || _deserializeLogged >= 150)
        {
            return false;
        }
        string subtype = Str(region, "m_AiSubTypeSpawned");
        return subtype == "Wolf" || subtype == "Bear";
    }

    internal static void LogDeserializeIn(SpawnRegion region, string text)
    {
        if (!ShouldLogDeserialize(region))
        {
            return;
        }
        _deserializeLogged++;
        string shown = text == null ? "null" : (text.Length > 400 ? text.Substring(0, 400) + "..." : text);
        Log($"DESERIALIZE-IN | region={region.gameObject.name} | activeBefore={region.gameObject.activeSelf} | saved={shown}");
    }

    internal static void LogDeserializeGameDone(SpawnRegion region)
    {
        if (!Enabled || region == null || _deserializeLogged >= 150)
        {
            return;
        }
        string subtype = Str(region, "m_AiSubTypeSpawned");
        if (subtype != "Wolf" && subtype != "Bear")
        {
            return;
        }
        Log($"DESERIALIZE-GAME-DONE | region={region.gameObject.name} | activeAfterGame={region.gameObject.activeSelf}");
    }

    // Prints every public instance property of obj that is a primitive/enum/string; other types show as <TypeName>.
    internal static string DumpValues(object obj)
    {
        var sb = new StringBuilder();
        foreach (PropertyInfo p in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
            if (p.GetIndexParameters().Length > 0)
            {
                continue;
            }

            string val;
            try
            {
                object v = p.GetValue(obj);
                if (v == null)
                {
                    val = "null";
                }
                else if (p.PropertyType.IsPrimitive || p.PropertyType.IsEnum || v is string)
                {
                    val = v.ToString() ?? "";
                }
                else
                {
                    val = $"<{p.PropertyType.Name}>";
                }
            }
            catch (Exception e)
            {
                val = "ERR:" + e.GetType().Name;
            }
            sb.Append($"{p.Name}={val}; ");
        }
        return sb.ToString();
    }

    private static void DumpType(Type t)
    {
        try
        {
            Log($"===== TYPE {t.FullName} (base: {t.BaseType?.FullName}) =====");

            foreach (ConstructorInfo c in t.GetConstructors(BindingFlags.Public | BindingFlags.Instance))
            {
                Log($"  CTOR({FormatParams(c.GetParameters())})");
            }

            foreach (PropertyInfo p in t.GetProperties(Declared))
            {
                Log($"  PROP {p.PropertyType.Name} {p.Name}");
            }

            foreach (MethodInfo m in t.GetMethods(Declared))
            {
                if (m.IsSpecialName)
                {
                    continue; // skip property getters/setters
                }
                Log($"  METHOD {m.ReturnType.Name} {m.Name}({FormatParams(m.GetParameters())})");
            }

            foreach (Type nested in t.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic))
            {
                Log($"  NESTED {nested.FullName}");
            }

            Log($"===== END TYPE {t.Name} =====");
        }
        catch (Exception e)
        {
            Log($"DumpType failed for {t.Name}: {e.Message}");
        }
    }

    private static string FormatParams(ParameterInfo[] ps)
    {
        var parts = new List<string>();
        foreach (ParameterInfo p in ps)
        {
            parts.Add($"{p.ParameterType.Name} {p.Name}");
        }
        return string.Join(", ", parts);
    }
}

// Runs BEFORE the real mod's own SpawnRegion.Start postfix (Priority.First), so an exception in the
// old Make*/Set* code can't stop the dump from running.
[HarmonyPatch(typeof(SpawnRegion), nameof(SpawnRegion.Start))]
internal class SpawnListDiagnosticPatch
{
    [HarmonyPriority(Priority.First)]
    private static void Postfix(SpawnRegion __instance)
    {
        SpawnListDiagnostic.Inspect(__instance);
    }
}

// These two run LAST, after the mod's own postfixes, so they show the state PSS leaves behind.
[HarmonyPatch(typeof(SpawnRegion), nameof(SpawnRegion.Start))]
internal class SpawnListDiagnosticStartAfter
{
    [HarmonyPriority(Priority.Last)]
    private static void Postfix(SpawnRegion __instance)
    {
        SpawnListDiagnostic.PostChangeReport(__instance, "Start");
    }
}

[HarmonyPatch(typeof(SpawnRegion), nameof(SpawnRegion.Deserialize))]
internal class SpawnListDiagnosticDeserializeAfter
{
    [HarmonyPriority(Priority.Last)]
    private static void Postfix(SpawnRegion __instance)
    {
        SpawnListDiagnostic.PostChangeReport(__instance, "Deserialize");
    }
}

[HarmonyPatch(typeof(SpawnRegion), nameof(SpawnRegion.InstantiateSpawnFromSaveData))]
internal class SpawnListDiagnosticSaveInstantiate
{
    private static void Postfix(SpawnRegion __instance, SpawnDataProxy spawnData, BaseAi __result)
    {
        SpawnListDiagnostic.LogSaveInstantiate(__instance, spawnData, __result);
    }
}

[HarmonyPatch(typeof(SpawnRegion), nameof(SpawnRegion.AttemptInstantiateAndPlaceSpawnFromSave))]
internal class SpawnListDiagnosticPendingRespawn
{
    private static void Postfix(SpawnRegion __instance, PendingSerializedRespawnInfo pendingSerializedRespawnInfo, BaseAi __result)
    {
        SpawnListDiagnostic.LogPendingRespawn(__instance, pendingSerializedRespawnInfo, __result);
    }
}

// Runs before the game's own Deserialize (Priority.First): shows the saved text and the region's state going in.
[HarmonyPatch(typeof(SpawnRegion), nameof(SpawnRegion.Deserialize))]
internal class SpawnListDiagnosticDeserializeIn
{
    [HarmonyPriority(Priority.First)]
    private static void Prefix(SpawnRegion __instance, string text)
    {
        SpawnListDiagnostic.LogDeserializeIn(__instance, text);
    }

    // Runs right after the game's Deserialize and BEFORE the mod's own postfix, so it shows what the game itself did.
    [HarmonyPriority(Priority.First)]
    private static void Postfix(SpawnRegion __instance)
    {
        SpawnListDiagnostic.LogDeserializeGameDone(__instance);
    }
}