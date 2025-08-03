using Il2Cpp;
using UnityEngine;

namespace PredatorSpawnSelector;

internal static class CougarManager
{
    internal static void HandleCougarTerritoriesForCurrentScene()
    {
        string sceneName = GameManager.m_ActiveScene;
        if (string.IsNullOrEmpty(sceneName)) return;

        MelonLoader.MelonLogger.Msg($"Handling cougar territories for scene: {sceneName}");

        switch (sceneName)
        {
            case "AshCanyonRegion":
                DisableAshCanyonCougars();
                break;
            case "BlackrockRegion":
            case "BlackrockPrisonZone":
                DisableBlackrockCougars();
                break;
            case "CanneryRegion":
                DisableCanneryCougars();
                break;
            case "AirfieldRegion":
                DisableAirfieldCougars();
                break;
            case "RiverValleyRegion":
                DisableRiverValleyCougars();
                break;
            case "MountainTownRegion":
                DisableMountainTownCougars();
                break;
            case "LakeRegion":
                DisableLakeRegionCougars();
                break;
            case "RuralRegion":
                DisableRuralRegionCougars();
                break;
            case "MountainPassRegion":
                DisableMountainPassCougars();
                break;
            case "CrashMountainRegion":
                DisableTimberwolfMountainCougars();
                break;
            default:
                MelonLoader.MelonLogger.Msg($"No cougar handling implementation for scene: {sceneName}");
                break;
        }
    }

    private static void DisableAshCanyonCougars()
    {
        bool t1Disabled = Settings.instance.disableAshCanyon1 || Settings.instance.disableAllCougars;
        bool t2Disabled = Settings.instance.disableAshCanyon2 || Settings.instance.disableAllCougars;
        bool t3Disabled = Settings.instance.disableAshCanyon3 || Settings.instance.disableAllCougars;

        if (t1Disabled)
            Patches.SafeDisable("Root/Design/Cougar/AttackZoneArea/CougarTerritoryZone_a_T1", "Ash Canyon Territory 1");
        if (t2Disabled)
            Patches.SafeDisable("Root/Design/Cougar/AttackZoneArea/CougarTerritoryZone_a_T2", "Ash Canyon Territory 2");
        if (t3Disabled)
            Patches.SafeDisable("Root/Design/Cougar/AttackZoneArea/CougarTerritoryZone_a_T3", "Ash Canyon Territory 3");
    }

    private static void DisableBlackrockCougars()
    {
        bool t1Disabled = Settings.instance.disableBlackrock1 || Settings.instance.disableAllCougars;
        bool t2Disabled = Settings.instance.disableBlackrock2 || Settings.instance.disableAllCougars;
        bool t3Disabled = Settings.instance.disableBlackrock3 || Settings.instance.disableAllCougars;

        if (t1Disabled)
        {
            Patches.SafeDisable("Design/Cougar/AttackZones/AttackZoneArea_a/CougarTerritoryZone_a_T1", "Blackrock Territory 1");
            Patches.SafeDisable("Design/Cougar/Cougar_RockScene_Prefab", "Blackrock Cougar Rock Scene");
        }
        if (t2Disabled)
            Patches.SafeDisable("Design/Cougar/AttackZones/AttackZoneArea_a/CougarTerritoryZone_a_T2", "Blackrock Territory 2");
        if (t3Disabled)
            Patches.SafeDisable("Design/Cougar/AttackZones/AttackZoneArea_a/CougarTerritoryZone_a_T3", "Blackrock Territory 3");
    }

    private static void DisableCanneryCougars()
    {
        bool t1Disabled = Settings.instance.disableBleak1 || Settings.instance.disableAllCougars;
        bool t2Disabled = Settings.instance.disableBleak2 || Settings.instance.disableAllCougars;
        bool t3Disabled = Settings.instance.disableBleak3 || Settings.instance.disableAllCougars;

        if (t1Disabled)
            Patches.SafeDisable("Root/Design/Cougar/AttackZones/AttackZoneArea_a/CougarTerritoryZone_a_T1", "Bleak Inlet Territory 1");
        if (t2Disabled)
            Patches.SafeDisable("Root/Design/Cougar/AttackZones/AttackZoneArea_a/CougarTerritoryZone_a_T2", "Bleak Inlet Territory 2");
        if (t3Disabled)
            Patches.SafeDisable("Root/Design/Cougar/AttackZones/AttackZoneArea_a/CougarTerritoryZone_a_T3", "Bleak Inlet Territory 3");
    }

    private static void DisableAirfieldCougars()
    {
        bool t1Disabled = Settings.instance.disableAirfield1 || Settings.instance.disableAllCougars;
        bool t2Disabled = Settings.instance.disableAirfield2 || Settings.instance.disableAllCougars;
        bool t3Disabled = Settings.instance.disableAirfield3 || Settings.instance.disableAllCougars;

        if (t1Disabled)
            Patches.SafeDisable("Cougar/AttackZones/CougarTerritoryZone_a_T1", "Forsaken Airfield Territory 1");
        if (t2Disabled)
            Patches.SafeDisable("Cougar/AttackZones/CougarTerritoryZone_a_T2", "Forsaken Airfield Territory 2");
        if (t3Disabled)
            Patches.SafeDisable("Cougar/AttackZones/CougarTerritoryZone_a_T3", "Forsaken Airfield Territory 3");
    }

    private static void DisableRiverValleyCougars()
    {
        bool t1Disabled = Settings.instance.disableRiverValley1 || Settings.instance.disableAllCougars;
        bool t2Disabled = Settings.instance.disableRiverValley2 || Settings.instance.disableAllCougars;
        bool t3Disabled = Settings.instance.disableRiverValley3 || Settings.instance.disableAllCougars;

        if (t1Disabled)
            Patches.SafeDisable("Design/Cougar/AttackZones/CougarTerritoryZone_a_T1", "Hushed River Valley Territory 1");
        if (t2Disabled)
            Patches.SafeDisable("Design/Cougar/AttackZones/CougarTerritoryZone_a_T2", "Hushed River Valley Territory 2");
        if (t3Disabled)
            Patches.SafeDisable("Design/Cougar/AttackZones/CougarTerritoryZone_a_T3", "Hushed River Valley Territory 3");
    }

    private static void DisableMountainTownCougars()
    {
        bool t1Disabled = Settings.instance.disableMountainTown1 || Settings.instance.disableAllCougars;
        bool t2Disabled = Settings.instance.disableMountainTown2 || Settings.instance.disableAllCougars;
        bool t3Disabled = Settings.instance.disableMountainTown3 || Settings.instance.disableAllCougars;

        if (t1Disabled)
            Patches.SafeDisable("Design/Cougar/AttackZoneAreas/CougarTerritoryZone_a_T1", "Mountain Town Territory 1");
        if (t2Disabled)
            Patches.SafeDisable("Design/Cougar/AttackZoneAreas/CougarTerritoryZone_a_T2", "Mountain Town Territory 2");
        if (t3Disabled)
            Patches.SafeDisable("Design/Cougar/AttackZoneAreas/CougarTerritoryZone_a_T3", "Mountain Town Territory 3");
    }

    private static void DisableLakeRegionCougars()
    {
        bool t1Disabled = Settings.instance.disableMysteryLake1 || Settings.instance.disableAllCougars;
        bool t2Disabled = Settings.instance.disableMysteryLake2 || Settings.instance.disableAllCougars;
        bool t3Disabled = Settings.instance.disableMysteryLake3 || Settings.instance.disableAllCougars;

        if (t1Disabled)
            Patches.SafeDisable("Design/Cougar/AttackZoneArea_a/CougarTerritoryZone_a_T1", "Mystery Lake Territory 1");
        if (t2Disabled)
            Patches.SafeDisable("Design/Cougar/AttackZoneArea_a/CougarTerritoryZone_a_T2", "Mystery Lake Territory 2");
        if (t3Disabled)
            Patches.SafeDisable("Design/Cougar/AttackZoneArea_a/CougarTerritoryZone_a_T3", "Mystery Lake Territory 3");
    }

    private static void DisableRuralRegionCougars()
    {
        bool t1Disabled = Settings.instance.disablePleasantValley1 || Settings.instance.disableAllCougars;
        bool t2Disabled = Settings.instance.disablePleasantValley2 || Settings.instance.disableAllCougars;
        bool t3Disabled = Settings.instance.disablePleasantValley3 || Settings.instance.disableAllCougars;

        if (t1Disabled)
            Patches.SafeDisable("Design/Cougar/AttackZoneArea_a/CougarTerritoryZone_a_T1", "Pleasant Valley Territory 1");
        if (t2Disabled)
            Patches.SafeDisable("Design/Cougar/AttackZoneArea_a/CougarTerritoryZone_a_T2", "Pleasant Valley Territory 2");
        if (t3Disabled)
            Patches.SafeDisable("Design/Cougar/AttackZoneArea_a/CougarTerritoryZone_a_T3", "Pleasant Valley Territory 3");
    }

    private static void DisableMountainPassCougars()
    {
        bool t1Disabled = Settings.instance.disableMountainPass1 || Settings.instance.disableAllCougars;
        bool t2Disabled = Settings.instance.disableMountainPass2 || Settings.instance.disableAllCougars;
        bool t3Disabled = Settings.instance.disableMountainPass3 || Settings.instance.disableAllCougars;

        if (t1Disabled)
            Patches.SafeDisable("Root/Wildlife_Spawns/Cougar/AttackZones/CougarTerritoryZone_a_T1", "Mountain Pass Territory 1");
        if (t2Disabled)
            Patches.SafeDisable("Root/Wildlife_Spawns/Cougar/AttackZones/CougarTerritoryZone_a_T2", "Mountain Pass Territory 2");
        if (t3Disabled)
            Patches.SafeDisable("Root/Wildlife_Spawns/Cougar/AttackZones/CougarTerritoryZone_a_T3", "Mountain Pass Territory 3");
    }

    private static void DisableTimberwolfMountainCougars()
    {
        bool t1Disabled = Settings.instance.disableTimberwolfMountain1 || Settings.instance.disableAllCougars;
        bool t2Disabled = Settings.instance.disableTimberwolfMountain2 || Settings.instance.disableAllCougars;
        bool t3Disabled = Settings.instance.disableTimberwolfMountain3 || Settings.instance.disableAllCougars;

        if (t1Disabled)
            Patches.SafeDisable("Design/Cougar/AttackZones/CougarTerritoryZone_a_T1", "Timberwolf Mountain Territory 1");
        if (t2Disabled)
            Patches.SafeDisable("Design/Cougar/AttackZones/CougarTerritoryZone_a_T2", "Timberwolf Mountain Territory 2");
        if (t3Disabled)
            Patches.SafeDisable("Design/Cougar/AttackZones/CougarTerritoryZone_a_T3", "Timberwolf Mountain Territory 3");
    }
}