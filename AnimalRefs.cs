using Il2Cpp;
using MelonLoader;

namespace PredatorSpawnSelector;

/// <summary>
/// Asset GUIDs for the animals this mod can place on a SpawnRegion, plus the helper that applies them.
/// The game replaced SpawnRegion.m_SpawnablePrefab / m_AuroraSpawnablePrefab with a list of
/// SpawnRegion.SpawnListEntry (m_Normal, m_Aurora: AssetReferenceBaseAi, m_Weight).
///
/// GUIDs below were OBSERVED in-game (SpawnListDiagnostic, CrashMountainRegion, Nov 2026 build).
/// Empty string = not observed yet; the matching option logs a warning and leaves the region unchanged.
/// </summary>
internal static class AnimalRefs
{
    internal const string Wolf = "82363e3c6e12c1e43999f7fd089573be";                // WILDLIFE_Wolf
    internal const string WolfAurora = "1f5a36dd0b0998641bad680079e6ec3f";          // WILDLIFE_Wolf_aurora
    internal const string Timberwolf = "b7a0c842a2dbd35448a933a6248871b0";          // WILDLIFE_Wolf_grey
    internal const string TimberwolfAurora = "0dfb38d6999f2ee4da93415a0e730123";    // WILDLIFE_Wolf_grey_aurora
    internal const string Bear = "7657f31e830016c48b0bae5bc6f42145";                // WILDLIFE_Bear
    internal const string BearAurora = "69418020b171d6049821813111cc3706";          // WILDLIFE_Bear_aurora

    // From the Addressables catalog (catalog.json, Nov 2026 build): Assets/Prefabs/Wildlife/WILDLIFE_BearHuntedChallenge.prefab
    internal const string BearChallenge = "3290d7188412d0642939adab2ff72f74";       // WILDLIFE_BearHuntedChallenge

    // OBSERVED in-game (MiningRegion/SPAWNREGION_WolfStarving, Nov 2026 build). Not in the main catalog.json, so they are probably
    // in a separate catalog (likely DLC content).
    internal const string WolfStarving = "0ecb1b58a24fdec4e9794635c039af2e";        // WILDLIFE_Wolf_Starving
    internal const string WolfStarvingAurora = "1d7a7b423c0255e4d8c91dce31b4d0c0";  // WILDLIFE_Wolf_Starving_Aurora

    /// <summary>
    /// Points every entry of the region's spawn list at the given animals.
    /// Returns true if anything was changed (false if already set, or if it could not be applied).
    /// </summary>
    internal static bool TrySetSpawn(SpawnRegion region, string normalGuid, string auroraGuid, string label)
    {
        if (string.IsNullOrEmpty(normalGuid) || string.IsNullOrEmpty(auroraGuid))
        {
            MelonLogger.Warning($"[AnimalRefs] No GUID known for {label}; leaving {region.gameObject.name} unchanged");
            return false;
        }

        var list = region.m_SpawnList;
        if (list == null || list.Count == 0)
        {
            MelonLogger.Warning($"[AnimalRefs] {region.gameObject.name} has an empty spawn list; cannot apply {label}");
            return false;
        }

        bool changed = false;
        for (int i = 0; i < list.Count; i++)
        {
            SpawnRegion.SpawnListEntry entry = list[i];
            if (entry == null)
            {
                continue;
            }

            if (entry.m_Normal?.AssetGUID == normalGuid && entry.m_Aurora?.AssetGUID == auroraGuid)
            {
                continue; // already set (Deserialize and Start both call us)
            }

            entry.m_Normal = new AssetReferenceBaseAi(normalGuid);
            entry.m_Aurora = new AssetReferenceBaseAi(auroraGuid);
            changed = true;
        }

        return changed;
    }
}