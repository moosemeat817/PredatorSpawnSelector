using Il2Cpp;
using Il2CppTLD.AI;
using UnityEngine;

namespace PredatorSpawnSelector;

internal static class BearManager
{
    internal static void AdjustBearToRegionSetting(SpawnRegion spawnRegion)
    {
        //MelonLoader.MelonLogger.Msg($"[BearManager.AdjustBearToRegionSetting] Processing region: {spawnRegion.gameObject.name} in scene: {GameManager.m_ActiveScene}");

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
                //MelonLoader.MelonLogger.Warning($"[BearManager.AdjustBearToRegionSetting] Unknown Bear Region ({GameManager.m_ActiveScene}) - no changes applied");
                break;
        }
    }

    // Handle all bear spawn regions for the current scene
    internal static void HandleBearSpawnRegionsForCurrentScene()
    {
        //MelonLoader.MelonLogger.Msg($"[BearManager.HandleBearSpawnRegionsForCurrentScene] Starting bear spawn region handling for scene: {GameManager.m_ActiveScene}");

        if (Settings.instance.disableAllBears)
        {
            int disabledCount = DisableAllBearSpawnRegions();
            //MelonLoader.MelonLogger.Msg($"[BearManager.HandleBearSpawnRegionsForCurrentScene] Total bear regions disabled: {disabledCount}");
        }
        else
        {
            //MelonLoader.MelonLogger.Msg("[BearManager.HandleBearSpawnRegionsForCurrentScene] Bear disabling is not enabled, skipping");
        }
    }

    private static int DisableAllBearSpawnRegions()
    {
        // Find all SpawnRegion components that spawn bears
        SpawnRegion[] allSpawnRegions = Resources.FindObjectsOfTypeAll<SpawnRegion>();
        int disabledCount = 0;

        //MelonLoader.MelonLogger.Msg($"[DisableAllBearSpawnRegions] Searching through {allSpawnRegions.Length} SpawnRegions");

        foreach (SpawnRegion spawnRegion in allSpawnRegions)
        {
            // Check if this spawn region is for bears
            if (spawnRegion.m_AiSubTypeSpawned == AiSubType.Bear)
            {
                //bool wasActive = spawnRegion.gameObject.activeSelf;
                RegionStateKeeper.Disable(spawnRegion);
                disabledCount++;
                //MelonLoader.MelonLogger.Msg($"[DisableAllBearSpawnRegions] Disabled bear spawn region: {spawnRegion.gameObject.name} (Was Active: {wasActive})");
            }
        }

        return disabledCount;
    }

    private static void SetBearType(SpawnRegion spawnRegion, BearSpawnType bearType)
    {
        //MelonLoader.MelonLogger.Msg($"[SetBearType] Setting bear type to {bearType} for region: {spawnRegion.gameObject.name}");

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
                //MelonLoader.MelonLogger.Msg($"[SetBearType] Disabling spawn region: {spawnRegion.gameObject.name}");
                RegionStateKeeper.Disable(spawnRegion);
                break;
        }
    }

    private static void MakeRegularBears(SpawnRegion spawnRegion)
    {
        if (AnimalRefs.TrySetSpawn(spawnRegion, AnimalRefs.Bear, AnimalRefs.BearAurora, "regular bears"))
        {
            //MelonLoader.MelonLogger.Msg($"[MakeRegularBears] Changed {spawnRegion.gameObject.name} to regular bears");
        }
    }

    private static void MakeChallengeBears(SpawnRegion spawnRegion)
    {
        // Challenge Bears don't have separate aurora variants, so use the same for both
        if (AnimalRefs.TrySetSpawn(spawnRegion, AnimalRefs.BearChallenge, AnimalRefs.BearChallenge, "challenge bears"))
        {
            //MelonLoader.MelonLogger.Msg($"[MakeChallengeBears] Changed {spawnRegion.gameObject.name} to challenge bears");

            // Set m_IgnoreCriticalHits to false for challenge bears
            SetChallengeBearCriticalHits(spawnRegion);
        }
    }

    private static void MakeAuroraBears(SpawnRegion spawnRegion)
    {
        // Use aurora bears for both normal and aurora spawns
        if (AnimalRefs.TrySetSpawn(spawnRegion, AnimalRefs.BearAurora, AnimalRefs.BearAurora, "aurora bears"))
        {
            //MelonLoader.MelonLogger.Msg($"[MakeAuroraBears] Changed {spawnRegion.gameObject.name} to aurora bears");
        }
    }

    private static void SetChallengeBearCriticalHits(SpawnRegion spawnRegion)
    {
        try
        {
            var list = spawnRegion.m_SpawnList;
            for (int i = 0; i < list.Count; i++)
            {
                GameObject challengeBear = list[i]?.m_Normal?.GetOrLoadTypedAsset()?.gameObject;
                AiBearChallengeHunted challengeHuntedComponent = challengeBear?.GetComponent<AiBearChallengeHunted>();
                if (challengeHuntedComponent != null)
                {
                    // Set m_IgnoreCriticalHits to false to allow critical hits
                    challengeHuntedComponent.m_IgnoreCriticalHits = false;
                }
            }
        }
        catch (System.Exception ex)
        {
            //MelonLoader.MelonLogger.Error($"[SetChallengeBearCriticalHits] Error setting challenge bear critical hits: {ex.Message}");
        }
    }
}