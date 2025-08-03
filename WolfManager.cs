using Il2Cpp;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace PredatorSpawnSelector;

internal static class WolfManager
{
    internal static void AdjustToRegionSetting(SpawnRegion spawnRegion)
    {
        switch (GameManager.m_ActiveScene)
        {
            case "AshCanyonRegion":
                SetWolfType(spawnRegion, Settings.instance.ashCanyonWolves);
                break;
            case "BlackrockRegion":
                SetWolfType(spawnRegion, Settings.instance.blackRockWolves);
                break;
            case "BlackrockPrisonZone":
                SetWolfType(spawnRegion, Settings.instance.blackRockPrisonWolves);
                break;
            case "CanneryRegion":
                SetWolfType(spawnRegion, Settings.instance.bleakInletWolves);
                break;
            case "MaintenanceShedB_SANDBOX":
                SetWolfType(spawnRegion, Settings.instance.bleakInletWorkshopWolves);
                break;
            case "TracksRegion":
                SetWolfType(spawnRegion, Settings.instance.brokenRailroadWolves);
                break;
            case "CoastalRegion":
                SetWolfType(spawnRegion, Settings.instance.coastalHighwayWolves);
                break;
            case "HighwayTransitionZone":
                SetWolfType(spawnRegion, Settings.instance.crumblingHighwayWolves);
                break;
            case "WhalingStationRegion":
                SetWolfType(spawnRegion, Settings.instance.desolationPointWolves);
                break;
            case "CaveC":
                SetWolfType(spawnRegion, Settings.instance.desolationPointCaveWolves);
                break;
            case "MarshRegion":
                SetWolfType(spawnRegion, Settings.instance.forlornMuskegWolves);
                break;
            case "RiverValleyRegion":
                SetWolfType(spawnRegion, Settings.instance.hushedRiverValleyWolves);
                break;
            case "CanyonRoadTransitionZone":
                SetWolfType(spawnRegion, Settings.instance.keepersPassWolves);
                break;
            case "MiningRegion":
                SetWolfType(spawnRegion, Settings.instance.miningRegionWolves);
                break;
            case "MountainPassRegion":
                SetWolfType(spawnRegion, Settings.instance.mountainPassWolves);
                break;
            case "MountainTownRegion":
                SetWolfType(spawnRegion, Settings.instance.mountainTownWolves);
                break;
            case "LakeRegion":
                SetWolfType(spawnRegion, Settings.instance.mysteryLakeWolves);
                break;
            case "RuralRegion":
                SetWolfType(spawnRegion, Settings.instance.pleasantValleyWolves);
                break;
            case "CrashMountainRegion":
                SetWolfType(spawnRegion, Settings.instance.timberwolfMountainWolves);
                break;
            case "DamRiverTransitionZoneB":
                SetWolfType(spawnRegion, Settings.instance.windingRiverWolves);
                break;
            case "LongRailTransitionZone":
                SetWolfType(spawnRegion, Settings.instance.farRangeBranchLineWolves);
                break;
            case "AirfieldRegion":
                SetWolfType(spawnRegion, Settings.instance.forsakenAirfieldWolves);
                break;
            default:
                MelonLoader.MelonLogger.Warning("Unknown Wolf Region (" + GameManager.m_ActiveScene + ") - no changes applied");
                break;
        }
    }

    // Handle all wolf spawn regions for the current scene
    internal static void HandleWolfSpawnRegionsForCurrentScene()
    {
        if (Settings.instance.disableAllRegularWolves)
        {
            DisableRegularWolfSpawnRegions();
        }

        if (Settings.instance.disableAllTimberWolves)
        {
            DisableTimberWolfSpawnRegions();
        }

        if (Settings.instance.disableAllStarvingWolves)
        {
            DisableStarvingWolfSpawnRegions();
        }
    }

    private static void DisableRegularWolfSpawnRegions()
    {
        // Find all GameObjects containing "SPAWNREGION_Wolf" in their name
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("SPAWNREGION_Wolf") && !obj.name.Contains("SPAWNREGION_Timberwolf") && !obj.name.Contains("SPAWNREGION_WolfStarving"))
            {
                obj.SetActive(false);
                MelonLoader.MelonLogger.Msg($"Disabled regular wolf spawn region: {obj.name}");
            }
        }
    }

    private static void DisableTimberWolfSpawnRegions()
    {
        // Find all GameObjects containing "SPAWNREGION_Timberwolf" in their name
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("SPAWNREGION_Timberwolf"))
            {
                obj.SetActive(false);
                MelonLoader.MelonLogger.Msg($"Disabled timberwolf spawn region: {obj.name}");
            }
        }
    }

    private static void DisableStarvingWolfSpawnRegions()
    {
        // Find all GameObjects containing "SPAWNREGION_WolfStarving" in their name
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("SPAWNREGION_WolfStarving"))
            {
                obj.SetActive(false);
                MelonLoader.MelonLogger.Msg($"Disabled starving wolf spawn region: {obj.name}");
            }
        }
    }

    private static void SetWolfType(SpawnRegion spawnRegion, SpawnType spawnType)
    {
        switch (spawnType)
        {
            case SpawnType.RegularWolves:
                MakeRegularWolves(spawnRegion);
                break;
            case SpawnType.Timberwolves:
                MakeTimberwolves(spawnRegion);
                break;
            case SpawnType.PoisonWolves:
                MakePoisonWolves(spawnRegion);
                break;
            case SpawnType.Random:
                MakeRandomWolves(spawnRegion);
                break;
            case SpawnType.None:
                // Disable the GameObject containing this spawn region
                spawnRegion.gameObject.SetActive(false);
                break;
        }
    }

    private static void MakeTimberwolves(SpawnRegion spawnRegion)
    {
        GameObject? timberwolf = Addressables.LoadAssetAsync<GameObject>("WILDLIFE_Wolf_grey").WaitForCompletion();
        GameObject? timberwolf_aurora = Addressables.LoadAssetAsync<GameObject>("WILDLIFE_Wolf_grey_aurora").WaitForCompletion();
        if (timberwolf && timberwolf_aurora && spawnRegion.m_SpawnablePrefab.name != timberwolf.name)
        {
            spawnRegion.m_AuroraSpawnablePrefab = timberwolf_aurora;
            spawnRegion.m_SpawnablePrefab = timberwolf;
            AdjustTimberwolfPackSize(spawnRegion);
        }
    }

    private static void MakeRegularWolves(SpawnRegion spawnRegion)
    {
        GameObject? regularWolf = Addressables.LoadAssetAsync<GameObject>("WILDLIFE_Wolf").WaitForCompletion();
        GameObject? regularWolf_aurora = Addressables.LoadAssetAsync<GameObject>("WILDLIFE_Wolf_aurora").WaitForCompletion();
        if (regularWolf && regularWolf_aurora && spawnRegion.m_SpawnablePrefab.name != regularWolf.name)
        {
            spawnRegion.m_AuroraSpawnablePrefab = regularWolf_aurora;
            spawnRegion.m_SpawnablePrefab = regularWolf;
        }
    }

    private static void MakePoisonWolves(SpawnRegion spawnRegion)
    {
        GameObject? poisonWolf = Addressables.LoadAssetAsync<GameObject>("WILDLIFE_Wolf_starving").WaitForCompletion();
        if (poisonWolf && spawnRegion.m_SpawnablePrefab.name != poisonWolf.name)
        {
            spawnRegion.m_AuroraSpawnablePrefab = poisonWolf; // Use same for aurora
            spawnRegion.m_SpawnablePrefab = poisonWolf;
        }
    }

    private static void MakeRandomWolves(SpawnRegion spawnRegion)
    {
        if (Utils.RollChance(Settings.instance.regularWolfPercentage))
        {
            MakeRegularWolves(spawnRegion);
        }
        else
        {
            MakeTimberwolves(spawnRegion);
        }
    }

    private static void AdjustTimberwolfPackSize(SpawnRegion spawnRegion)
    {
        spawnRegion.m_MaxSimultaneousSpawnsDayInterloper = NotOne(spawnRegion.m_MaxSimultaneousSpawnsDayInterloper);
        spawnRegion.m_MaxSimultaneousSpawnsDayPilgrim = NotOne(spawnRegion.m_MaxSimultaneousSpawnsDayPilgrim);
        spawnRegion.m_MaxSimultaneousSpawnsDayStalker = NotOne(spawnRegion.m_MaxSimultaneousSpawnsDayStalker);
        spawnRegion.m_MaxSimultaneousSpawnsDayVoyageur = NotOne(spawnRegion.m_MaxSimultaneousSpawnsDayVoyageur);
        spawnRegion.m_MaxSimultaneousSpawnsNightInterloper = NotOne(spawnRegion.m_MaxSimultaneousSpawnsNightInterloper);
        spawnRegion.m_MaxSimultaneousSpawnsNightPilgrim = NotOne(spawnRegion.m_MaxSimultaneousSpawnsNightPilgrim);
        spawnRegion.m_MaxSimultaneousSpawnsNightStalker = NotOne(spawnRegion.m_MaxSimultaneousSpawnsNightStalker);
        spawnRegion.m_MaxSimultaneousSpawnsNightVoyageur = NotOne(spawnRegion.m_MaxSimultaneousSpawnsNightVoyageur);
    }

    private static int NotOne(int num)
    {
        return num == 1 ? 2 : num;
    }
}