using Il2Cpp;
using Il2CppTLD.AI;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace PredatorSpawnSelector;

internal static class BearManager
{
    internal static void AdjustBearToRegionSetting(SpawnRegion spawnRegion)
    {
        MelonLoader.MelonLogger.Msg($"[BearManager.AdjustBearToRegionSetting] Processing region: {spawnRegion.gameObject.name} in scene: {GameManager.m_ActiveScene}");

        switch (GameManager.m_ActiveScene)
        {
            case "AshCanyonRegion":
                SetBearType(spawnRegion, Settings.instance.ashCanyonBears);
                break;
            case "BlackrockRegion":
                SetBearType(spawnRegion, Settings.instance.blackRockBears);
                break;
            case "BlackrockPrisonZone":
                SetBearType(spawnRegion, Settings.instance.blackRockPrisonBears);
                break;
            case "CanneryRegion":
                SetBearType(spawnRegion, Settings.instance.bleakInletBears);
                break;
            case "TracksRegion":
                SetBearType(spawnRegion, Settings.instance.brokenRailroadBears);
                break;
            case "CoastalRegion":
                SetBearType(spawnRegion, Settings.instance.coastalHighwayBears);
                break;
            case "WhalingStationRegion":
                SetBearType(spawnRegion, Settings.instance.desolationPointBears);
                break;
            case "MarshRegion":
                SetBearType(spawnRegion, Settings.instance.forlornMuskegBears);
                break;
            case "RiverValleyRegion":
                SetBearType(spawnRegion, Settings.instance.hushedRiverValleyBears);
                break;
            case "MiningRegion":
                SetBearType(spawnRegion, Settings.instance.miningRegionBears);
                break;
            case "MountainPassRegion":
                SetBearType(spawnRegion, Settings.instance.mountainPassBears);
                break;
            case "MountainTownRegion":
                SetBearType(spawnRegion, Settings.instance.mountainTownBears);
                break;
            case "LakeRegion":
                SetBearType(spawnRegion, Settings.instance.mysteryLakeBears);
                break;
            case "RuralRegion":
                SetBearType(spawnRegion, Settings.instance.pleasantValleyBears);
                break;
            case "CrashMountainRegion":
                SetBearType(spawnRegion, Settings.instance.timberwolfMountainBears);
                break;
            case "AirfieldRegion":
                SetBearType(spawnRegion, Settings.instance.forsakenAirfieldBears);
                break;
            default:
                MelonLoader.MelonLogger.Warning($"[BearManager.AdjustBearToRegionSetting] Unknown Bear Region ({GameManager.m_ActiveScene}) - no changes applied");
                break;
        }
    }

    // Handle all bear spawn regions for the current scene
    internal static void HandleBearSpawnRegionsForCurrentScene()
    {
        MelonLoader.MelonLogger.Msg($"[BearManager.HandleBearSpawnRegionsForCurrentScene] Starting bear spawn region handling for scene: {GameManager.m_ActiveScene}");

        if (Settings.instance.disableAllBears)
        {
            int disabledCount = DisableAllBearSpawnRegions();
            MelonLoader.MelonLogger.Msg($"[BearManager.HandleBearSpawnRegionsForCurrentScene] Total bear regions disabled: {disabledCount}");
        }
        else
        {
            MelonLoader.MelonLogger.Msg("[BearManager.HandleBearSpawnRegionsForCurrentScene] Bear disabling is not enabled, skipping");
        }
    }

    private static int DisableAllBearSpawnRegions()
    {
        // Find all SpawnRegion components that spawn bears
        SpawnRegion[] allSpawnRegions = Resources.FindObjectsOfTypeAll<SpawnRegion>();
        int disabledCount = 0;

        MelonLoader.MelonLogger.Msg($"[DisableAllBearSpawnRegions] Searching through {allSpawnRegions.Length} SpawnRegions");

        foreach (SpawnRegion spawnRegion in allSpawnRegions)
        {
            // Check if this spawn region is for bears
            if (spawnRegion.m_AiSubTypeSpawned == AiSubType.Bear)
            {
                bool wasActive = spawnRegion.gameObject.activeSelf;
                spawnRegion.gameObject.SetActive(false);
                disabledCount++;
                MelonLoader.MelonLogger.Msg($"[DisableAllBearSpawnRegions] Disabled bear spawn region: {spawnRegion.gameObject.name} (Was Active: {wasActive})");
            }
        }

        return disabledCount;
    }

    private static void SetBearType(SpawnRegion spawnRegion, BearSpawnType bearType)
    {
        MelonLoader.MelonLogger.Msg($"[SetBearType] Setting bear type to {bearType} for region: {spawnRegion.gameObject.name}");

        switch (bearType)
        {
            case BearSpawnType.RegularBears:
                MakeRegularBears(spawnRegion);
                break;
            case BearSpawnType.ChallengeBears:
                MakeChallengeBears(spawnRegion);
                break;
            case BearSpawnType.AuroraBears:
                MakeAuroraBears(spawnRegion);
                break;
            case BearSpawnType.None:
                MelonLoader.MelonLogger.Msg($"[SetBearType] Disabling spawn region: {spawnRegion.gameObject.name}");
                spawnRegion.gameObject.SetActive(false);
                break;
        }
    }

    private static void MakeRegularBears(SpawnRegion spawnRegion)
    {
        GameObject? regularBear = Addressables.LoadAssetAsync<GameObject>("WILDLIFE_Bear").WaitForCompletion();
        GameObject? regularBear_aurora = Addressables.LoadAssetAsync<GameObject>("WILDLIFE_Bear_aurora").WaitForCompletion();
        if (regularBear && regularBear_aurora && spawnRegion.m_SpawnablePrefab.name != regularBear.name)
        {
            spawnRegion.m_SpawnablePrefab = regularBear;
            spawnRegion.m_AuroraSpawnablePrefab = regularBear_aurora;
            MelonLoader.MelonLogger.Msg($"[MakeRegularBears] Changed {spawnRegion.gameObject.name} to regular bears");
        }
    }

    private static void MakeChallengeBears(SpawnRegion spawnRegion)
    {
        GameObject? challengeBear = Addressables.LoadAssetAsync<GameObject>("WILDLIFE_BearHuntedChallenge").WaitForCompletion();
        if (challengeBear && spawnRegion.m_SpawnablePrefab.name != challengeBear.name)
        {
            spawnRegion.m_SpawnablePrefab = challengeBear;
            spawnRegion.m_AuroraSpawnablePrefab = challengeBear; // Challenge Bears don't have separate aurora variants

            MelonLoader.MelonLogger.Msg($"[MakeChallengeBears] Changed {spawnRegion.gameObject.name} to challenge bears");

            // Set m_IgnoreCriticalHits to false for challenge bears
            SetChallengeBearCriticalHits(challengeBear);
        }
    }

    private static void MakeAuroraBears(SpawnRegion spawnRegion)
    {
        GameObject? auroraBear = Addressables.LoadAssetAsync<GameObject>("WILDLIFE_Bear_aurora").WaitForCompletion();
        if (auroraBear && spawnRegion.m_SpawnablePrefab.name != auroraBear.name)
        {
            // Use aurora bears for both normal and aurora spawns
            spawnRegion.m_SpawnablePrefab = auroraBear;
            spawnRegion.m_AuroraSpawnablePrefab = auroraBear;
            MelonLoader.MelonLogger.Msg($"[MakeAuroraBears] Changed {spawnRegion.gameObject.name} to aurora bears");
        }
    }

    private static void SetChallengeBearCriticalHits(GameObject challengeBear)
    {
        try
        {
            // Get the AiBearChallengeHunted component
            AiBearChallengeHunted? challengeHuntedComponent = challengeBear.GetComponent<AiBearChallengeHunted>();
            if (challengeHuntedComponent != null)
            {
                // Set m_IgnoreCriticalHits to false to allow critical hits
                challengeHuntedComponent.m_IgnoreCriticalHits = false;
                MelonLoader.MelonLogger.Msg($"[SetChallengeBearCriticalHits] Set challenge bear {challengeBear.name} to allow critical hits (m_IgnoreCriticalHits = false)");
            }
            else
            {
                MelonLoader.MelonLogger.Warning($"[SetChallengeBearCriticalHits] Could not find AiBearChallengeHunted component on challenge bear prefab: {challengeBear.name}");
            }
        }
        catch (System.Exception ex)
        {
            MelonLoader.MelonLogger.Error($"[SetChallengeBearCriticalHits] Error setting challenge bear critical hits: {ex.Message}");
        }
    }
}