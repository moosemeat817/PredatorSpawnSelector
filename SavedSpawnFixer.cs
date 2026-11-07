using HarmonyLib;
using Il2Cpp;
using Il2CppTLD.AI;
using MelonLoader;

namespace PredatorSpawnSelector;

/// <summary>
/// After the Nov 2026 update, animals stored in a save remember which prefab they were
/// (SpawnDataProxy.m_AssetReferenceGUID). SpawnRegion.InstantiateSpawnFromSaveData rebuilds them from that,
/// so changing the region's spawn list only affected NEW spawns. Before the update, reloading was enough.
///
/// This patch runs just before a saved animal is rebuilt. If its saved animal is not one of the animals the
/// region's (possibly PSS-modified) spawn list allows, it is swapped for one the region would pick today.
/// Regions left on "Default" are untouched, because their saved animal is already in the original list.
/// </summary>
internal static class SavedSpawnFixer
{
    private const bool LogReplacements = true;

    internal static void Fix(SpawnRegion region, SpawnDataProxy spawnData)
    {
        try
        {
            if (region == null || spawnData == null)
            {
                return;
            }

            if (region.m_AiSubTypeSpawned != AiSubType.Wolf && region.m_AiSubTypeSpawned != AiSubType.Bear)
            {
                return;
            }

            string savedGuid = spawnData.m_AssetReferenceGUID?.AssetGUID;
            if (string.IsNullOrEmpty(savedGuid))
            {
                return;
            }

            var list = region.m_SpawnList;
            if (list == null || list.Count == 0)
            {
                return;
            }

            for (int i = 0; i < list.Count; i++)
            {
                SpawnRegion.SpawnListEntry entry = list[i];
                if (entry == null)
                {
                    continue;
                }

                if (entry.m_Normal?.AssetGUID == savedGuid || entry.m_Aurora?.AssetGUID == savedGuid)
                {
                    return; // saved animal is still allowed in this region
                }
            }

            AssetReferenceBaseAi replacement = region.PickSpawnAnimal(region.m_WildlifeMode);
            string newGuid = replacement?.AssetGUID;
            if (string.IsNullOrEmpty(newGuid) || newGuid == savedGuid)
            {
                return;
            }

            spawnData.m_AssetReferenceGUID = replacement;

            if (LogReplacements && Settings.instance.enableDiagnostics)
            {
                MelonLogger.Msg($"[SavedSpawnFixer] {region.gameObject.name}: saved animal {savedGuid} replaced with {newGuid}");
            }
        }
        catch (System.Exception ex)
        {
            MelonLogger.Warning($"[SavedSpawnFixer] Could not adjust saved spawn: {ex.Message}");
        }
    }
}

[HarmonyPatch(typeof(SpawnRegion), nameof(SpawnRegion.InstantiateSpawnFromSaveData))]
internal class SavedSpawnFixerPatch
{
    private static void Prefix(SpawnRegion __instance, SpawnDataProxy spawnData)
    {
        SavedSpawnFixer.Fix(__instance, spawnData);
    }
}