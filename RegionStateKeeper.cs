using System.Collections.Generic;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace PredatorSpawnSelector;

/// <summary>
/// The game saves each SpawnRegion's on/off state in its save text ("m_IsActive":true; the field is left out when
/// the region is inactive, and the game treats a missing field as off). When this mod disabled a region with
/// SetActive(false), that "off" was saved, and it stayed off after the option was switched back on.
///
/// This class makes the mod's disabling invisible to the save:
///   1. Disable() remembers what state the region had before the mod switched it off.
///   2. A Deserialize prefix records the state the save itself says the region had.
///   3. A Serialize postfix rewrites "m_IsActive" in the saved text of any region the mod disabled this session,
///      so the save keeps the region's original state instead of "off".
/// Regions the mod did not disable are never touched, so vanilla's own random on/off rolls are unaffected.
///
/// Saves that already recorded a region as off (from before this fix) cannot be repaired this way,
/// because the original state was never written down.
/// </summary>
internal static class RegionStateKeeper
{
    private const string ActiveTrue = "\"m_IsActive\":true";
    private const string ActiveFalse = "\"m_IsActive\":false";

    private static readonly Dictionary<int, bool> VanillaActive = new Dictionary<int, bool>();
    private static readonly HashSet<int> DisabledByUs = new HashSet<int>();
    private static int _fixesLogged;

    /// <summary>Disables a spawn region while remembering its original on/off state.</summary>
    internal static void Disable(SpawnRegion region)
    {
        if (region == null)
        {
            return;
        }

        int id = region.GetInstanceID();
        if (!VanillaActive.ContainsKey(id))
        {
            // Fallback for regions with no saved text (e.g. new game): the state right before the first disable.
            VanillaActive[id] = region.gameObject.activeSelf;
        }
        DisabledByUs.Add(id);
        region.gameObject.SetActive(false);
    }

    /// <summary>For code that finds regions by GameObject name: use Disable() if it is a SpawnRegion.</summary>
    internal static void DisableObject(GameObject obj)
    {
        if (obj == null)
        {
            return;
        }

        SpawnRegion region = obj.GetComponent<SpawnRegion>();
        if (region != null)
        {
            Disable(region);
        }
        else
        {
            obj.SetActive(false);
        }
    }

    /// <summary>Called with the text the game is about to restore a region from.</summary>
    internal static void RecordSaved(SpawnRegion region, string savedText)
    {
        if (region == null || string.IsNullOrEmpty(savedText))
        {
            return;
        }

        // What the save says is the region's real state, whatever this mod did to it earlier this session.
        VanillaActive[region.GetInstanceID()] = savedText.Contains(ActiveTrue);
    }

    /// <summary>Called with the text the game just produced when saving a region.</summary>
    internal static void FixSerialized(SpawnRegion region, ref string json)
    {
        try
        {
            if (region == null || string.IsNullOrEmpty(json))
            {
                return;
            }

            int id = region.GetInstanceID();
            if (!DisabledByUs.Contains(id))
            {
                return;
            }

            bool originallyActive = VanillaActive.TryGetValue(id, out bool v) && v;
            string before = json;

            if (originallyActive)
            {
                if (json.Contains(ActiveTrue))
                {
                    return;
                }

                if (json.Contains(ActiveFalse))
                {
                    json = json.Replace(ActiveFalse, ActiveTrue);
                }
                else
                {
                    int brace = json.IndexOf('{');
                    if (brace < 0)
                    {
                        return;
                    }
                    string rest = json.Substring(brace + 1);
                    bool restIsEmpty = rest.TrimStart().StartsWith("}");
                    json = json.Substring(0, brace + 1) + ActiveTrue + (restIsEmpty ? "" : ",") + rest;
                }
            }
            else if (json.Contains(ActiveTrue))
            {
                json = json.Replace(ActiveTrue, ActiveFalse);
            }

            if (json != before && _fixesLogged < 5 && Settings.instance.enableDiagnostics)
            {
                _fixesLogged++;
                MelonLogger.Msg($"[RegionStateKeeper] Saved {region.gameObject.name} with its original on/off state (active={originallyActive})");
            }
        }
        catch (System.Exception ex)
        {
            MelonLogger.Warning($"[RegionStateKeeper] Could not adjust saved region text: {ex.Message}");
        }
    }
}

[HarmonyPatch(typeof(SpawnRegion), nameof(SpawnRegion.Deserialize))]
internal class RegionStateKeeperDeserialize
{
    [HarmonyPriority(Priority.First)]
    private static void Prefix(SpawnRegion __instance, string text)
    {
        RegionStateKeeper.RecordSaved(__instance, text);
    }
}

[HarmonyPatch(typeof(SpawnRegion), nameof(SpawnRegion.Serialize))]
internal class RegionStateKeeperSerialize
{
    private static void Postfix(SpawnRegion __instance, ref string __result)
    {
        RegionStateKeeper.FixSerialized(__instance, ref __result);
    }
}