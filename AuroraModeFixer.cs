using HarmonyLib;
using Il2Cpp;
using Il2CppTLD.AI;

namespace PredatorSpawnSelector;

/// <summary>
/// The aurora wolf / bear prefabs carry BaseAi.m_WildlifeMode = Aurora. The "Aurora ..." options put those prefabs
/// in BOTH slots of a region's spawn list, so a region running in Normal mode spawns an animal whose own mode says
/// Aurora. The game then treats it as an "other mode" animal (SpawnRegion.AdjustOtherModePopulation removes it and
/// spawns a replacement), which showed up as a remove / respawn loop several times per second.
///
/// Fix: right after the game creates (or restores) a wolf/bear, set its m_WildlifeMode to the mode the region
/// asked for, so the region considers it one of its own.
/// </summary>
internal static class AuroraModeFixer
{
    internal static void Align(SpawnRegion region, BaseAi ai, WildlifeMode mode)
    {
        try
        {
            if (region == null || ai == null)
            {
                return;
            }

            if (region.m_AiSubTypeSpawned != AiSubType.Wolf && region.m_AiSubTypeSpawned != AiSubType.Bear)
            {
                return;
            }

            if (ai.m_WildlifeMode != mode)
            {
                ai.m_WildlifeMode = mode;
            }
        }
        catch (System.Exception ex)
        {
            MelonLoader.MelonLogger.Warning($"[AuroraModeFixer] Could not align wildlife mode: {ex.Message}");
        }
    }
}

[HarmonyPatch(typeof(SpawnRegion), nameof(SpawnRegion.InstantiateSpawnInternal))]
internal class AuroraModeFixerInternal
{
    [HarmonyPriority(Priority.First)]
    private static void Postfix(SpawnRegion __instance, WildlifeMode wildlifetype, BaseAi __result)
    {
        AuroraModeFixer.Align(__instance, __result, wildlifetype);
    }
}

[HarmonyPatch(typeof(SpawnRegion), nameof(SpawnRegion.InstantiateSpawn))]
internal class AuroraModeFixerSpawn
{
    [HarmonyPriority(Priority.First)]
    private static void Postfix(SpawnRegion __instance, WildlifeMode wildlifeMode, BaseAi __result)
    {
        AuroraModeFixer.Align(__instance, __result, wildlifeMode);
    }
}

[HarmonyPatch(typeof(SpawnRegion), nameof(SpawnRegion.InstantiateSpawnFromSaveData))]
internal class AuroraModeFixerFromSave
{
    [HarmonyPriority(Priority.First)]
    private static void Postfix(SpawnRegion __instance, WildlifeMode wildlifeMode, BaseAi __result)
    {
        AuroraModeFixer.Align(__instance, __result, wildlifeMode);
    }
}
