using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace PredatorSpawnSelector;

// TEMPORARY DIAGNOSTIC for the "aurora wolves flicker and bounce" problem. Delete with SpawnListDiagnostic.cs when done.
// Logs (with timestamps) every time a SpawnRegion adds, removes, instantiates or mode-checks a WOLF, and what
// components an aurora wolf carries. Output goes to UserData/PSS_SpawnListDiagnostic.txt (lines start with AURORA-DIAG).
internal static class AuroraDiagnostic
{
    private const bool CompiledIn = true;
    private static readonly Dictionary<string, int> _counts = new Dictionary<string, int>();
    private static readonly HashSet<string> _componentDumps = new HashSet<string>();

    internal static bool Allow(string bucket, int cap)
    {
        if (!CompiledIn || !SpawnListDiagnostic.Enabled)
        {
            return false;
        }
        _counts.TryGetValue(bucket, out int n);
        if (n >= cap)
        {
            return false;
        }
        _counts[bucket] = n + 1;
        return true;
    }

    internal static bool IsWolf(SpawnRegion region)
    {
        return region != null && region.m_AiSubTypeSpawned == AiSubType.Wolf;
    }

    internal static void Say(string msg)
    {
        SpawnListDiagnostic.Log("AURORA-DIAG | " + msg);
    }


    // Counts every wolf/bear object in the loaded scene (active AND inactive) every 10 seconds, grouped by
    // prefab name + wildlife mode, so the log can be compared with what Unity Explorer shows.
    internal static IEnumerator Census()
    {
        for (int i = 1; i <= 12; i++)
        {
            yield return new WaitForSeconds(10f);
            try
            {
                if (SpawnListDiagnostic.Enabled)
                {
                    CensusOnce(i);
                }
            }
            catch (Exception ex)
            {
                Say("census failed: " + ex.Message);
            }
        }
    }

    private static void CensusOnce(int n)
    {
        var groups = new Dictionary<string, int[]>(); // [active, inactive]
        var parents = new Dictionary<string, int>();
        int total = 0;

        foreach (BaseAi ai in Resources.FindObjectsOfTypeAll<BaseAi>())
        {
            if (ai == null || ai.gameObject == null || !ai.gameObject.scene.IsValid())
            {
                continue; // skip prefab assets that are not in the scene
            }
            if (ai.m_AiSubType != AiSubType.Wolf && ai.m_AiSubType != AiSubType.Bear)
            {
                continue;
            }

            total++;
            string baseName = Regex.Replace(ai.gameObject.name.Replace("(Clone)", ""), @"_\d+$", "");
            string key = $"{baseName} mode={ai.m_WildlifeMode}";
            if (!groups.TryGetValue(key, out int[] counts))
            {
                counts = new int[2];
                groups[key] = counts;
            }
            counts[ai.gameObject.activeInHierarchy ? 0 : 1]++;

            string parent = ai.m_SpawnRegionParent == null ? "none" : ai.m_SpawnRegionParent.gameObject.name;
            parents.TryGetValue(parent, out int pc);
            parents[parent] = pc + 1;
        }

        var sb = new StringBuilder();
        foreach (var kv in groups)
        {
            sb.Append($"{kv.Key} active={kv.Value[0]} inactive={kv.Value[1]}; ");
        }
        var pb = new StringBuilder();
        foreach (var kv in parents)
        {
            pb.Append($"{kv.Key}={kv.Value}; ");
        }
        Say($"CENSUS #{n} | total={total} | {sb}");
        Say($"CENSUS #{n} by region | {pb}");
    }

    internal static void DumpComponentsOnce(BaseAi ai)
    {
        try
        {
            string name = ai.gameObject.name;
            string key = System.Text.RegularExpressions.Regex.Replace(name.Replace("(Clone)", ""), @"_\d+$", "");
            if (!_componentDumps.Add(key))
            {
                return;
            }

            var sb = new StringBuilder();
            foreach (Component c in ai.gameObject.GetComponents<Component>())
            {
                if (c == null)
                {
                    continue;
                }
                string enabledText = "";
                if (c is Behaviour b)
                {
                    enabledText = b.enabled ? "(on)" : "(off)";
                }
                sb.Append(c.GetType().Name).Append(enabledText).Append(", ");
            }
            Say($"COMPONENTS of {name}: {sb}");
            Say($"BASEAI-VALUES of {name}: {SpawnListDiagnostic.DumpValues(ai)}");
        }
        catch (Exception ex)
        {
            Say("component dump failed: " + ex.Message);
        }
    }
}

[HarmonyPatch(typeof(SpawnRegion), nameof(SpawnRegion.InstantiateSpawnInternal))]
internal class AuroraDiagInstantiate
{
    [HarmonyPriority(Priority.Last)]
    private static void Postfix(SpawnRegion __instance, AssetReferenceBaseAi animalAssetReference, WildlifeMode wildlifetype, BaseAi __result)
    {
        if (AuroraDiagnostic.Allow("census", 1))
        {
            MelonLoader.MelonCoroutines.Start(AuroraDiagnostic.Census());
        }

        if (!AuroraDiagnostic.IsWolf(__instance) || !AuroraDiagnostic.Allow("instantiate", 60))
        {
            return;
        }
        string guid = animalAssetReference?.AssetGUID;
        AuroraDiagnostic.Say($"INSTANTIATE | region={__instance.gameObject.name} | animal={SpawnListDiagnostic.FriendlyName(guid)} | mode={wildlifetype} | regionMode={__instance.m_WildlifeMode} | result={(__result == null ? "null" : __result.gameObject.name)} | aiMode={(__result == null ? "n/a" : __result.m_WildlifeMode.ToString())}");
        if (__result != null)
        {
            AuroraDiagnostic.DumpComponentsOnce(__result);
        }
    }
}

[HarmonyPatch(typeof(SpawnRegion), nameof(SpawnRegion.HasSameWildlifeMode))]
internal class AuroraDiagHasSameMode
{
    private static void Postfix(SpawnRegion __instance, BaseAi baseAi, WildlifeMode wildlifeMode, bool __result)
    {
        // Only the "no" answers are interesting; "yes" would flood the log.
        if (__result || !AuroraDiagnostic.IsWolf(__instance) || !AuroraDiagnostic.Allow("hassame", 60))
        {
            return;
        }
        AuroraDiagnostic.Say($"HasSameWildlifeMode=FALSE | region={__instance.gameObject.name} | ai={(baseAi == null ? "null" : baseAi.gameObject.name)} | askedMode={wildlifeMode} | regionMode={__instance.m_WildlifeMode}");
    }
}

[HarmonyPatch(typeof(SpawnRegion), nameof(SpawnRegion.RemoveActiveSpawns))]
internal class AuroraDiagRemove
{
    private static void Postfix(SpawnRegion __instance, int numToDeActivate, WildlifeMode wildlifeMode, bool isAdjustingOtherWildlifeMode)
    {
        if (!AuroraDiagnostic.IsWolf(__instance) || !AuroraDiagnostic.Allow("remove", 60))
        {
            return;
        }
        AuroraDiagnostic.Say($"REMOVE | region={__instance.gameObject.name} | num={numToDeActivate} | mode={wildlifeMode} | adjustingOtherMode={isAdjustingOtherWildlifeMode} | regionMode={__instance.m_WildlifeMode}");
    }
}

[HarmonyPatch(typeof(SpawnRegion), nameof(SpawnRegion.AddActiveSpawns))]
internal class AuroraDiagAdd
{
    private static void Postfix(SpawnRegion __instance, int numToActivate, WildlifeMode wildlifeMode)
    {
        if (!AuroraDiagnostic.IsWolf(__instance) || !AuroraDiagnostic.Allow("add", 60))
        {
            return;
        }
        AuroraDiagnostic.Say($"ADD | region={__instance.gameObject.name} | num={numToActivate} | mode={wildlifeMode} | regionMode={__instance.m_WildlifeMode}");
    }
}

[HarmonyPatch(typeof(SpawnRegion), nameof(SpawnRegion.OnAuroraEnabled))]
internal class AuroraDiagOnAurora
{
    private static void Postfix(SpawnRegion __instance, bool enabled)
    {
        if (!AuroraDiagnostic.IsWolf(__instance) || !AuroraDiagnostic.Allow("onaurora", 40))
        {
            return;
        }
        AuroraDiagnostic.Say($"OnAuroraEnabled({enabled}) | region={__instance.gameObject.name} | regionMode={__instance.m_WildlifeMode}");
    }
}

[HarmonyPatch(typeof(SpawnRegion), nameof(SpawnRegion.MaybeSuspendForAurora))]
internal class AuroraDiagSuspend
{
    private static void Postfix(SpawnRegion __instance)
    {
        if (!AuroraDiagnostic.IsWolf(__instance) || !AuroraDiagnostic.Allow("suspend", 30))
        {
            return;
        }
        AuroraDiagnostic.Say($"MaybeSuspendForAurora | region={__instance.gameObject.name} | disabledByAurora={__instance.m_HasBeenDisabledByAurora}");
    }
}

[HarmonyPatch(typeof(SpawnRegion), nameof(SpawnRegion.MaybeResumeAfterAurora))]
internal class AuroraDiagResume
{
    private static void Postfix(SpawnRegion __instance)
    {
        if (!AuroraDiagnostic.IsWolf(__instance) || !AuroraDiagnostic.Allow("resume", 30))
        {
            return;
        }
        AuroraDiagnostic.Say($"MaybeResumeAfterAurora | region={__instance.gameObject.name} | disabledByAurora={__instance.m_HasBeenDisabledByAurora}");
    }
}