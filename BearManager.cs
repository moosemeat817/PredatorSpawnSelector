using Il2Cpp;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace PredatorSpawnSelector;

internal static class BearManager
{
    internal static void AdjustBearToRegionSetting(SpawnRegion spawnRegion)
    {
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
                MelonLoader.MelonLogger.Warning("Unknown Bear Region (" + GameManager.m_ActiveScene + ") - no changes applied");
                break;
        }
    }

    // Handle all bear spawn regions for the current scene
    internal static void HandleBearSpawnRegionsForCurrentScene()
    {
        if (Settings.instance.disableAllBears)
        {
            DisableAllBearSpawnRegions();
        }
    }

    private static void DisableAllBearSpawnRegions()
    {
        // Find all SpawnRegion components that spawn bears
        SpawnRegion[] allSpawnRegions = Resources.FindObjectsOfTypeAll<SpawnRegion>();

        foreach (SpawnRegion spawnRegion in allSpawnRegions)
        {
            // Check if this spawn region is for bears
            if (spawnRegion.m_AiSubTypeSpawned == AiSubType.Bear)
            {
                spawnRegion.gameObject.SetActive(false);
                MelonLoader.MelonLogger.Msg($"Disabled bear spawn region: {spawnRegion.gameObject.name}");
            }
        }
    }

    private static void SetBearType(SpawnRegion spawnRegion, BearSpawnType bearType)
    {
        switch (bearType)
        {
            case BearSpawnType.RegularBears:
                MakeRegularBears(spawnRegion);
                break;
            case BearSpawnType.ChallengeBears:
                MakeChallengeBears(spawnRegion);
                break;
            case BearSpawnType.None:
                // Disable the GameObject containing this spawn region
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
        }
    }

    private static void MakeChallengeBears(SpawnRegion spawnRegion)
    {
        GameObject? challengeBear = Addressables.LoadAssetAsync<GameObject>("WILDLIFE_BearHuntedChallenge").WaitForCompletion();
        if (challengeBear && spawnRegion.m_SpawnablePrefab.name != challengeBear.name)
        {
            spawnRegion.m_SpawnablePrefab = challengeBear;
            spawnRegion.m_AuroraSpawnablePrefab = challengeBear; // Challenge Bears don't have separate aurora variants
        }
    }
}