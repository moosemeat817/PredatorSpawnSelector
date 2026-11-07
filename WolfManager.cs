using System.Collections.Generic;
using Il2Cpp;
using UnityEngine;

namespace PredatorSpawnSelector;

internal static class WolfManager
{
    // Caches the Random roll outcome per SpawnRegion so that repeated calls
    // (Deserialize + Start both trigger AdjustToRegionSetting for the same region)
    // don't re-roll and potentially flip the result. Cleared on scene load.
    private static readonly Dictionary<int, WolfSpawnType> _randomRollCache = new Dictionary<int, WolfSpawnType>();

    internal static void ClearRandomRollCache()
    {
        _randomRollCache.Clear();
    }

    internal static void AdjustToRegionSetting(SpawnRegion spawnRegion)
    {
        //MelonLoader.MelonLogger.Msg($"[WolfManager.AdjustToRegionSetting] Processing region: {spawnRegion.gameObject.name} in scene: {GameManager.m_ActiveScene}");

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
                //MelonLoader.MelonLogger.Warning($"[WolfManager.AdjustToRegionSetting] Unknown Wolf Region ({GameManager.m_ActiveScene}) - no changes applied");
                break;
        }
    }

    // Handle all wolf spawn regions for the current scene
    internal static void HandleWolfSpawnRegionsForCurrentScene()
    {
        //MelonLoader.MelonLogger.Msg($"[WolfManager.HandleWolfSpawnRegionsForCurrentScene] Starting wolf spawn region handling for scene: {GameManager.m_ActiveScene}");

        int regularDisabled = 0;
        int timberDisabled = 0;
        int starvingDisabled = 0;

        if (Settings.instance.disableAllRegularWolves)
        {
            //MelonLoader.MelonLogger.Msg("[WolfManager.HandleWolfSpawnRegionsForCurrentScene] Disabling all regular wolves");
            regularDisabled = DisableRegularWolfSpawnRegions();
        }

        if (Settings.instance.disableAllTimberWolves)
        {
            //MelonLoader.MelonLogger.Msg("[WolfManager.HandleWolfSpawnRegionsForCurrentScene] Disabling all timber wolves");
            timberDisabled = DisableTimberWolfSpawnRegions();
        }

        if (Settings.instance.disableAllStarvingWolves)
        {
            //MelonLoader.MelonLogger.Msg("[WolfManager.HandleWolfSpawnRegionsForCurrentScene] Disabling all starving wolves");
            starvingDisabled = DisableStarvingWolfSpawnRegions();
        }

        //MelonLoader.MelonLogger.Msg($"[WolfManager.HandleWolfSpawnRegionsForCurrentScene] Summary - Regular: {regularDisabled} disabled, Timber: {timberDisabled} disabled, Starving: {starvingDisabled} disabled");
    }

    private static int DisableRegularWolfSpawnRegions()
    {
        // Find all GameObjects containing "SPAWNREGION_Wolf" in their name
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        int disabledCount = 0;

        //MelonLoader.MelonLogger.Msg($"[DisableRegularWolfSpawnRegions] Searching through {allObjects.Length} GameObjects");

        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("SPAWNREGION_Wolf") && !obj.name.Contains("SPAWNREGION_Timberwolf") && !obj.name.Contains("SPAWNREGION_WolfStarving"))
            {
                bool wasActive = obj.activeSelf;
                RegionStateKeeper.DisableObject(obj);
                disabledCount++;
                //MelonLoader.MelonLogger.Msg($"[DisableRegularWolfSpawnRegions] Disabled regular wolf spawn region: {obj.name} (Was Active: {wasActive})");
            }
        }

        //MelonLoader.MelonLogger.Msg($"[DisableRegularWolfSpawnRegions] Total regular wolf regions disabled: {disabledCount}");
        return disabledCount;
    }

    private static int DisableTimberWolfSpawnRegions()
    {
        // Find all GameObjects containing "SPAWNREGION_Timberwolf" in their name
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        int disabledCount = 0;

        //MelonLoader.MelonLogger.Msg($"[DisableTimberWolfSpawnRegions] Searching through {allObjects.Length} GameObjects");

        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("SPAWNREGION_Timberwolf"))
            {
                bool wasActive = obj.activeSelf;
                RegionStateKeeper.DisableObject(obj);
                disabledCount++;
                //MelonLoader.MelonLogger.Msg($"[DisableTimberWolfSpawnRegions] Disabled timberwolf spawn region: {obj.name} (Was Active: {wasActive})");
            }
        }

        //MelonLoader.MelonLogger.Msg($"[DisableTimberWolfSpawnRegions] Total timberwolf regions disabled: {disabledCount}");
        return disabledCount;
    }

    private static int DisableStarvingWolfSpawnRegions()
    {
        // Find all GameObjects containing "SPAWNREGION_WolfStarving" in their name
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        int disabledCount = 0;

        //MelonLoader.MelonLogger.Msg($"[DisableStarvingWolfSpawnRegions] Searching through {allObjects.Length} GameObjects");

        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("SPAWNREGION_WolfStarving"))
            {
                bool wasActive = obj.activeSelf;
                RegionStateKeeper.DisableObject(obj);
                disabledCount++;
                //MelonLoader.MelonLogger.Msg($"[DisableStarvingWolfSpawnRegions] Disabled starving wolf spawn region: {obj.name} (Was Active: {wasActive})");
            }
        }

        //MelonLoader.MelonLogger.Msg($"[DisableStarvingWolfSpawnRegions] Total starving wolf regions disabled: {disabledCount}");
        return disabledCount;
    }

    private static void SetWolfType(SpawnRegion spawnRegion, WolfSpawnType spawnType)
    {
        //MelonLoader.MelonLogger.Msg($"[SetWolfType] Setting wolf type to {spawnType} for region: {spawnRegion.gameObject.name}");

        switch (spawnType)
        {
            case WolfSpawnType.RegularWolves:
                MakeRegularWolves(spawnRegion);
                break;
            case WolfSpawnType.Timberwolves:
                MakeTimberwolves(spawnRegion);
                break;
            case WolfSpawnType.StarvingWolves:
                MakeStarvingWolves(spawnRegion);
                break;
            case WolfSpawnType.AuroraRegularWolves:
                MakeAuroraRegularWolves(spawnRegion);
                break;
            case WolfSpawnType.AuroraTimberwolves:
                MakeAuroraTimberwolves(spawnRegion);
                break;
            case WolfSpawnType.AuroraStarvingWolves:
                MakeAuroraStarvingWolves(spawnRegion);
                break;
            case WolfSpawnType.Random:
                MakeRandomWolves(spawnRegion);
                break;
            case WolfSpawnType.None:
                //MelonLoader.MelonLogger.Msg($"[SetWolfType] Disabling spawn region: {spawnRegion.gameObject.name}");
                RegionStateKeeper.Disable(spawnRegion);
                break;
            case WolfSpawnType.Default:
                //MelonLoader.MelonLogger.Msg($"[SetWolfType] Using default wolf type for region: {spawnRegion.gameObject.name}");
                break;
        }
    }

    private static void MakeTimberwolves(SpawnRegion spawnRegion)
    {
        if (AnimalRefs.TrySetSpawn(spawnRegion, AnimalRefs.Timberwolf, AnimalRefs.TimberwolfAurora, "timberwolves"))
        {
            AdjustTimberwolfPackSize(spawnRegion);
            //MelonLoader.MelonLogger.Msg($"[MakeTimberwolves] Changed {spawnRegion.gameObject.name} to timberwolves");
        }
    }

    private static void MakeRegularWolves(SpawnRegion spawnRegion)
    {
        if (AnimalRefs.TrySetSpawn(spawnRegion, AnimalRefs.Wolf, AnimalRefs.WolfAurora, "regular wolves"))
        {
            //MelonLoader.MelonLogger.Msg($"[MakeRegularWolves] Changed {spawnRegion.gameObject.name} to regular wolves");
        }
    }

    private static void MakeStarvingWolves(SpawnRegion spawnRegion)
    {
        // Starving wolves have their own aurora variant
        if (AnimalRefs.TrySetSpawn(spawnRegion, AnimalRefs.WolfStarving, AnimalRefs.WolfStarvingAurora, "starving wolves"))
        {
            //MelonLoader.MelonLogger.Msg($"[MakeStarvingWolves] Changed {spawnRegion.gameObject.name} to starving wolves");
        }
    }

    private static void MakeAuroraTimberwolves(SpawnRegion spawnRegion)
    {
        // Use aurora timberwolves for both normal and aurora spawns
        if (AnimalRefs.TrySetSpawn(spawnRegion, AnimalRefs.TimberwolfAurora, AnimalRefs.TimberwolfAurora, "aurora timberwolves"))
        {
            AdjustTimberwolfPackSize(spawnRegion);
            //MelonLoader.MelonLogger.Msg($"[MakeAuroraTimberwolves] Changed {spawnRegion.gameObject.name} to aurora timberwolves");
        }
    }

    private static void MakeAuroraRegularWolves(SpawnRegion spawnRegion)
    {
        // Use aurora regular wolves for both normal and aurora spawns
        if (AnimalRefs.TrySetSpawn(spawnRegion, AnimalRefs.WolfAurora, AnimalRefs.WolfAurora, "aurora regular wolves"))
        {
            //MelonLoader.MelonLogger.Msg($"[MakeAuroraRegularWolves] Changed {spawnRegion.gameObject.name} to aurora regular wolves");
        }
    }

    private static void MakeAuroraStarvingWolves(SpawnRegion spawnRegion)
    {
        // Aurora starving wolves use the aurora variant for both normal and aurora spawns
        if (AnimalRefs.TrySetSpawn(spawnRegion, AnimalRefs.WolfStarvingAurora, AnimalRefs.WolfStarvingAurora, "aurora starving wolves"))
        {
            //MelonLoader.MelonLogger.Msg($"[MakeAuroraStarvingWolves] Changed {spawnRegion.gameObject.name} to aurora starving wolves");
        }
    }

    private static void MakeRandomWolves(SpawnRegion spawnRegion)
    {
        int regionId = spawnRegion.GetInstanceID();

        if (!_randomRollCache.TryGetValue(regionId, out WolfSpawnType chosenType))
        {
            chosenType = Utils.RollChance(Settings.instance.regularWolfPercentage)
                ? WolfSpawnType.RegularWolves
                : WolfSpawnType.Timberwolves;
            _randomRollCache[regionId] = chosenType;
        }

        if (chosenType == WolfSpawnType.RegularWolves)
        {
            MakeRegularWolves(spawnRegion);
            //MelonLoader.MelonLogger.Msg($"[MakeRandomWolves] Randomly selected regular wolves for {spawnRegion.gameObject.name}");
        }
        else
        {
            MakeTimberwolves(spawnRegion);
            //MelonLoader.MelonLogger.Msg($"[MakeRandomWolves] Randomly selected timberwolves for {spawnRegion.gameObject.name}");
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
        //MelonLoader.MelonLogger.Msg($"[AdjustTimberwolfPackSize] Adjusted pack sizes for timberwolves in {spawnRegion.gameObject.name}");
    }

    private static int NotOne(int num)
    {
        return num == 1 ? 2 : num;
    }
}

internal static class Utils
{
    /// <summary>
    /// Returns true with the given percent probability (0-100).
    /// e.g. RollChance(75f) returns true ~75% of the time.
    /// </summary>
    internal static bool RollChance(float percentChance)
    {
        return UnityEngine.Random.Range(0f, 100f) < percentChance;
    }
}