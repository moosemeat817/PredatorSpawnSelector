using ModSettings;
using System.Reflection;

namespace PredatorSpawnSelector;

internal sealed class Settings : JsonModSettings
{
    internal static Settings instance = new Settings();

    [Section("Visibility Controls")]

    [Name("Show Wolf Settings")]
    [Description("Toggle visibility of all wolf spawn settings")]
    public bool showWolfSettings = false;

    [Name("Show Bear Settings")]
    [Description("Toggle visibility of all bear spawn settings")]
    public bool showBearSettings = false;

    [Name("Show Cougar Settings")]
    [Description("Toggle visibility of all cougar spawn settings")]
    public bool showCougarSettings = false;

    [Section("Wolf Settings")]

    // ALL REGIONS
    [Name("**Disable All Regular Wolf Spawns")]
    [Description("Disables Regular Wolves in all regions.  This setting overrides the regional settings below.  NOTE: This does not disable other wolves that have been set to a different type below.")]
    public bool disableAllRegularWolves = false;

    [Name("**Disable All Timberwolf Spawns")]
    [Description("Disables Timberwolves in all regions.  This setting overrides the regional settings below.  NOTE: This does not disable other wolves that have been set to a different type below.")]
    public bool disableAllTimberWolves = false;

    [Name("**Disable All Starving Wolf Spawns")]
    [Description("Disables Starving Wolves in all regions.  This setting overrides the regional settings below.  NOTE: This does not disable other wolves that have been set to a different type below.")]
    public bool disableAllStarvingWolves = false;

    [Name("**Regular Wolf Percentage (to Timberwolves)")]
    [Description("If you use the random option in any of the settings below, this is the percent probability of a pack containing regular wolves (and the rest will be timberwolves).")]
    [Slider(0f, 100f, 101)]
    public float regularWolfPercentage = 50f;

    [Name("Ash Canyon")]
    [Description("Choose the type of wolves that spawn in this region")]
    [Choice("Default", "Timberwolves", "Regular Wolves", "Starving Wolves", "Aurora Timberwolves", "Aurora Regular Wolves", "Aurora Starving Wolves", "Random", "None")]
    public WolfSpawnType ashCanyonWolves = WolfSpawnType.Default;

    [Name("Blackrock")]
    [Description("Choose the type of wolves that spawn in this region")]
    [Choice("Default", "Timberwolves", "Regular Wolves", "Starving Wolves", "Aurora Timberwolves", "Aurora Regular Wolves", "Aurora Starving Wolves", "Random", "None")]
    public WolfSpawnType blackRockWolves = WolfSpawnType.Default;

    [Name("Blackrock Prison")]
    [Description("Choose the type of wolves that spawn in this region")]
    [Choice("Default", "Timberwolves", "Regular Wolves", "Starving Wolves", "Aurora Timberwolves", "Aurora Regular Wolves", "Aurora Starving Wolves", "Random", "None")]
    public WolfSpawnType blackRockPrisonWolves = WolfSpawnType.Default;

    [Name("Bleak Inlet")]
    [Description("Choose the type of wolves that spawn in this region")]
    [Choice("Default", "Timberwolves", "Regular Wolves", "Starving Wolves", "Aurora Timberwolves", "Aurora Regular Wolves", "Aurora Starving Wolves", "Random", "None")]
    public WolfSpawnType bleakInletWolves = WolfSpawnType.Default;

    [Name("Bleak Inlet Workshop")]
    [Description("Choose the type of wolves that spawn in this region")]
    [Choice("Default", "Timberwolves", "Regular Wolves", "Starving Wolves", "Aurora Timberwolves", "Aurora Regular Wolves", "Aurora Starving Wolves", "Random", "None")]
    public WolfSpawnType bleakInletWorkshopWolves = WolfSpawnType.Default;

    [Name("Broken Railroad")]
    [Description("Choose the type of wolves that spawn in this region")]
    [Choice("Default", "Timberwolves", "Regular Wolves", "Starving Wolves", "Aurora Timberwolves", "Aurora Regular Wolves", "Aurora Starving Wolves", "Random", "None")]
    public WolfSpawnType brokenRailroadWolves = WolfSpawnType.Default;

    [Name("Coastal Highway")]
    [Description("Choose the type of wolves that spawn in this region")]
    [Choice("Default", "Timberwolves", "Regular Wolves", "Starving Wolves", "Aurora Timberwolves", "Aurora Regular Wolves", "Aurora Starving Wolves", "Random", "None")]
    public WolfSpawnType coastalHighwayWolves = WolfSpawnType.Default;

    [Name("Crumbling Highway")]
    [Description("Choose the type of wolves that spawn in this region")]
    [Choice("Default", "Timberwolves", "Regular Wolves", "Starving Wolves", "Aurora Timberwolves", "Aurora Regular Wolves", "Aurora Starving Wolves", "Random", "None")]
    public WolfSpawnType crumblingHighwayWolves = WolfSpawnType.Default;

    [Name("Desolation Point")]
    [Description("Choose the type of wolves that spawn in this region")]
    [Choice("Default", "Timberwolves", "Regular Wolves", "Starving Wolves", "Aurora Timberwolves", "Aurora Regular Wolves", "Aurora Starving Wolves", "Random", "None")]
    public WolfSpawnType desolationPointWolves = WolfSpawnType.Default;

    [Name("Desolation Point Cave")]
    [Description("Choose the type of wolves that spawn in this region")]
    [Choice("Default", "Timberwolves", "Regular Wolves", "Starving Wolves", "Aurora Timberwolves", "Aurora Regular Wolves", "Aurora Starving Wolves", "Random", "None")]
    public WolfSpawnType desolationPointCaveWolves = WolfSpawnType.Default;

    [Name("Far Range Branch Line")]
    [Description("Choose the type of wolves that spawn in this region")]
    [Choice("Default", "Timberwolves", "Regular Wolves", "Starving Wolves", "Aurora Timberwolves", "Aurora Regular Wolves", "Aurora Starving Wolves", "Random", "None")]
    public WolfSpawnType farRangeBranchLineWolves = WolfSpawnType.Default;

    [Name("Forlorn Muskeg")]
    [Description("Choose the type of wolves that spawn in this region")]
    [Choice("Default", "Timberwolves", "Regular Wolves", "Starving Wolves", "Aurora Timberwolves", "Aurora Regular Wolves", "Aurora Starving Wolves", "Random", "None")]
    public WolfSpawnType forlornMuskegWolves = WolfSpawnType.Default;

    [Name("Forsaken Airfield")]
    [Description("Choose the type of wolves that spawn in this region")]
    [Choice("Default", "Timberwolves", "Regular Wolves", "Starving Wolves", "Aurora Timberwolves", "Aurora Regular Wolves", "Aurora Starving Wolves", "Random", "None")]
    public WolfSpawnType forsakenAirfieldWolves = WolfSpawnType.Default;

    [Name("Hushed River Valley")]
    [Description("Choose the type of wolves that spawn in this region")]
    [Choice("Default", "Timberwolves", "Regular Wolves", "Starving Wolves", "Aurora Timberwolves", "Aurora Regular Wolves", "Aurora Starving Wolves", "Random", "None")]
    public WolfSpawnType hushedRiverValleyWolves = WolfSpawnType.Default;

    [Name("Keeper's Pass")]
    [Description("Choose the type of wolves that spawn in this region")]
    [Choice("Default", "Timberwolves", "Regular Wolves", "Starving Wolves", "Aurora Timberwolves", "Aurora Regular Wolves", "Aurora Starving Wolves", "Random", "None")]
    public WolfSpawnType keepersPassWolves = WolfSpawnType.Default;

    [Name("Mining Region")]
    [Description("Choose the type of wolves that spawn in this region")]
    [Choice("Default", "Timberwolves", "Regular Wolves", "Starving Wolves", "Aurora Timberwolves", "Aurora Regular Wolves", "Aurora Starving Wolves", "Random", "None")]
    public WolfSpawnType miningRegionWolves = WolfSpawnType.Default;

    [Name("Sundered Pass")]
    [Description("Choose the type of wolves that spawn in this region")]
    [Choice("Default", "Timberwolves", "Regular Wolves", "Starving Wolves", "Aurora Timberwolves", "Aurora Regular Wolves", "Aurora Starving Wolves", "Random", "None")]
    public WolfSpawnType mountainPassWolves = WolfSpawnType.Default;

    [Name("Mountain Town")]
    [Description("Choose the type of wolves that spawn in this region")]
    [Choice("Default", "Timberwolves", "Regular Wolves", "Starving Wolves", "Aurora Timberwolves", "Aurora Regular Wolves", "Aurora Starving Wolves", "Random", "None")]
    public WolfSpawnType mountainTownWolves = WolfSpawnType.Default;

    [Name("Mystery Lake")]
    [Description("Choose the type of wolves that spawn in this region")]
    [Choice("Default", "Timberwolves", "Regular Wolves", "Starving Wolves", "Aurora Timberwolves", "Aurora Regular Wolves", "Aurora Starving Wolves", "Random", "None")]
    public WolfSpawnType mysteryLakeWolves = WolfSpawnType.Default;

    [Name("Pleasant Valley")]
    [Description("Choose the type of wolves that spawn in this region")]
    [Choice("Default", "Timberwolves", "Regular Wolves", "Starving Wolves", "Aurora Timberwolves", "Aurora Regular Wolves", "Aurora Starving Wolves", "Random", "None")]
    public WolfSpawnType pleasantValleyWolves = WolfSpawnType.Default;

    [Name("Timberwolf Mountain")]
    [Description("Choose the type of wolves that spawn in this region")]
    [Choice("Default", "Timberwolves", "Regular Wolves", "Starving Wolves", "Aurora Timberwolves", "Aurora Regular Wolves", "Aurora Starving Wolves", "Random", "None")]
    public WolfSpawnType timberwolfMountainWolves = WolfSpawnType.Default;

    [Name("Winding River")]
    [Description("Choose the type of wolves that spawn in this region")]
    [Choice("Default", "Timberwolves", "Regular Wolves", "Starving Wolves", "Aurora Timberwolves", "Aurora Regular Wolves", "Aurora Starving Wolves", "Random", "None")]
    public WolfSpawnType windingRiverWolves = WolfSpawnType.Default;

    [Section("Bear Settings")]

    // ALL REGIONS
    [Name("**Disable All Bear Spawns")]
    [Description("Disables bears in all regions.  This setting overrides all Bear settings below")]
    public bool disableAllBears = false;

    [Name("Ash Canyon Bears")]
    [Description("Choose the type of bears that spawn in this region")]
    [Choice("Regular Bears", "Challenge Bears", "Aurora Bears", "None")]
    public BearSpawnType ashCanyonBears = BearSpawnType.RegularBears;

    [Name("Blackrock Bears")]
    [Description("Choose the type of bears that spawn in this region")]
    [Choice("Regular Bears", "Challenge Bears", "Aurora Bears", "None")]
    public BearSpawnType blackRockBears = BearSpawnType.RegularBears;

    [Name("Blackrock Prison Bears")]
    [Description("Choose the type of bears that spawn in this region")]
    [Choice("Regular Bears", "Challenge Bears", "Aurora Bears", "None")]
    public BearSpawnType blackRockPrisonBears = BearSpawnType.RegularBears;

    [Name("Bleak Inlet Bears")]
    [Description("Choose the type of bears that spawn in this region")]
    [Choice("Regular Bears", "Challenge Bears", "Aurora Bears", "None")]
    public BearSpawnType bleakInletBears = BearSpawnType.RegularBears;

    [Name("Broken Railroad Bears")]
    [Description("Choose the type of bears that spawn in this region")]
    [Choice("Regular Bears", "Challenge Bears", "Aurora Bears", "None")]
    public BearSpawnType brokenRailroadBears = BearSpawnType.RegularBears;

    [Name("Coastal Highway Bears")]
    [Description("Choose the type of bears that spawn in this region")]
    [Choice("Regular Bears", "Challenge Bears", "Aurora Bears", "None")]
    public BearSpawnType coastalHighwayBears = BearSpawnType.RegularBears;

    [Name("Desolation Point Bears")]
    [Description("Choose the type of bears that spawn in this region")]
    [Choice("Regular Bears", "Challenge Bears", "Aurora Bears", "None")]
    public BearSpawnType desolationPointBears = BearSpawnType.RegularBears;

    [Name("Forlorn Muskeg Bears")]
    [Description("Choose the type of bears that spawn in this region")]
    [Choice("Regular Bears", "Challenge Bears", "Aurora Bears", "None")]
    public BearSpawnType forlornMuskegBears = BearSpawnType.RegularBears;

    [Name("Forsaken Airfield Bears")]
    [Description("Choose the type of bears that spawn in this region")]
    [Choice("Regular Bears", "Challenge Bears", "Aurora Bears", "None")]
    public BearSpawnType forsakenAirfieldBears = BearSpawnType.RegularBears;

    [Name("Hushed River Valley Bears")]
    [Description("Choose the type of bears that spawn in this region")]
    [Choice("Regular Bears", "Challenge Bears", "Aurora Bears", "None")]
    public BearSpawnType hushedRiverValleyBears = BearSpawnType.RegularBears;

    [Name("Mining Region Bears")]
    [Description("Choose the type of bears that spawn in this region")]
    [Choice("Regular Bears", "Challenge Bears", "Aurora Bears", "None")]
    public BearSpawnType miningRegionBears = BearSpawnType.RegularBears;

    [Name("Sundered Pass Bears")]
    [Description("Choose the type of bears that spawn in this region")]
    [Choice("Regular Bears", "Challenge Bears", "Aurora Bears", "None")]
    public BearSpawnType mountainPassBears = BearSpawnType.RegularBears;

    [Name("Mountain Town Bears")]
    [Description("Choose the type of bears that spawn in this region")]
    [Choice("Regular Bears", "Challenge Bears", "Aurora Bears", "None")]
    public BearSpawnType mountainTownBears = BearSpawnType.RegularBears;

    [Name("Mystery Lake Bears")]
    [Description("Choose the type of bears that spawn in this region")]
    [Choice("Regular Bears", "Challenge Bears", "Aurora Bears", "None")]
    public BearSpawnType mysteryLakeBears = BearSpawnType.RegularBears;

    [Name("Pleasant Valley Bears")]
    [Description("Choose the type of bears that spawn in this region")]
    [Choice("Regular Bears", "Challenge Bears", "Aurora Bears", "None")]
    public BearSpawnType pleasantValleyBears = BearSpawnType.RegularBears;

    [Name("Timberwolf Mountain Bears")]
    [Description("Choose the type of bears that spawn in this region")]
    [Choice("Regular Bears", "Challenge Bears", "Aurora Bears", "None")]
    public BearSpawnType timberwolfMountainBears = BearSpawnType.RegularBears;

    [Section("Cougar Settings")]

    // ALL REGIONS
    [Name("**Disable All Cougar Territories")]
    [Description("Disables cougars in all regions.  This setting overrides all Cougar settings below")]
    public bool disableAllCougars = false;

    // ASH CANYON
    [Name("Ash Canyon - Disable Territory 1")]
    [Description("Disable Cougar Territory Near Stone Shelf Cave")]
    public bool disableAshCanyon1 = false;

    [Name("     Ash Canyon - Disable Territory 2")]
    [Description("Disable Cougar Territory Near Angler's Den")]
    public bool disableAshCanyon2 = false;

    [Name("     Ash Canyon - Disable Territory 3")]
    [Description("Disable Cougar Territory Near Fishing Hut")]
    public bool disableAshCanyon3 = false;

    // BLACKROCK
    [Name("Blackrock - Disable Territory 1")]
    [Description("Disable Cougar Territory Near Preppers Cache")]
    public bool disableBlackrock1 = false;

    [Name("     Blackrock - Disable Territory 2")]
    [Description("Disable Cougar Territory Near Forager's Remnant")]
    public bool disableBlackrock2 = false;

    [Name("     Blackrock - Disable Territory 3")]
    [Description("Disable Cougar Territory Near Foreman's Clearcut")]
    public bool disableBlackrock3 = false;

    // BLEAK INLET
    [Name("Bleak Inlet - Disable Territory 1")]
    [Description("Disable Cougar Territory Near Alpha Bunker")]
    public bool disableBleak1 = false;

    [Name("     Bleak Inlet - Disable Territory 2")]
    [Description("Disable Cougar Territory Near Cannery Worker Residence")]
    public bool disableBleak2 = false;

    [Name("     Bleak Inlet - Disable Territory 3")]
    [Description("Disable Cougar Territory Near Washed Out Trailers")]
    public bool disableBleak3 = false;

    // FORSAKEN AIRFIELD
    [Name("Forsaken Airfield - Disable Territory 1")]
    [Description("Disable Cougar Territory Near Brittle Cave")]
    public bool disableAirfield1 = false;

    [Name("     Forsaken Airfield - Disable Territory 2")]
    [Description("Disable Cougar Territory Near Mindful Cabin")]
    public bool disableAirfield2 = false;

    [Name("     Forsaken Airfield - Disable Territory 3")]
    [Description("Disable Cougar Territory Near Fallow Dugout")]
    public bool disableAirfield3 = false;

    // HUSHED RIVER VALLEY
    [Name("Hushed River Valley - Disable Territory 1")]
    [Description("Disable Cougar Territory in NW Corner")]
    public bool disableRiverValley1 = false;

    [Name("     Hushed River Valley - Disable Territory 2")]
    [Description("Disable Cougar Territory Near Stairsteps Lake")]
    public bool disableRiverValley2 = false;

    [Name("     Hushed River Valley - Disable Territory 3")]
    [Description("Disable Cougar Territory (Third Territory)")]
    public bool disableRiverValley3 = false;

    // MOUNTAIN TOWN
    [Name("Mountain Town - Disable Territory 1")]
    [Description("Disable Cougar Territory Near Farmhouse")]
    public bool disableMountainTown1 = false;

    [Name("     Mountain Town - Disable Territory 2")]
    [Description("Disable Cougar Territory Near Church")]
    public bool disableMountainTown2 = false;

    [Name("     Mountain Town - Disable Territory 3")]
    [Description("Disable Cougar Territory Near the Orca")]
    public bool disableMountainTown3 = false;

    // MYSTERY LAKE
    [Name("Mystery Lake - Disable Territory 1")]
    [Description("Disable Cougar Territory Near the Clearcut")]
    public bool disableMysteryLake1 = false;

    [Name("     Mystery Lake - Disable Territory 2")]
    [Description("Disable Cougar Territory Near the Logging Area")]
    public bool disableMysteryLake2 = false;

    [Name("     Mystery Lake - Disable Territory 3")]
    [Description("Disable Cougar Territory Near Camp Office")]
    public bool disableMysteryLake3 = false;

    // PLEASANT VALLEY
    [Name("Pleasant Valley - Disable Territory 1")]
    [Description("Disable Cougar Territory Near Pensive Pond")]
    public bool disablePleasantValley1 = false;

    [Name("     Pleasant Valley - Disable Territory 2")]
    [Description("Disable Cougar Territory Near Farmhouse")]
    public bool disablePleasantValley2 = false;

    [Name("     Pleasant Valley - Disable Territory 3")]
    [Description("Disable Cougar Territory Near Thompson's Crossing")]
    public bool disablePleasantValley3 = false;

    // MOUNTAIN PASS (SUNDERED PASS)
    [Name("Sundered Pass - Disable Territory 1")]
    [Description("Disable Cougar Territory Near Giant's Thumbprint")]
    public bool disableMountainPass1 = false;

    [Name("     Mountain Pass - Disable Territory 2")]
    [Description("Disable Cougar Territory Near Seeding Spring")]
    public bool disableMountainPass2 = false;

    [Name("     Mountain Pass - Disable Territory 3")]
    [Description("Disable Cougar Territory (Third Territory)")]
    public bool disableMountainPass3 = false;

    // TIMBERWOLF MOUNTAIN
    [Name("Timberwolf Mountain - Disable Territory 1")]
    [Description("Disable Cougar Territory Near the Wing")]
    public bool disableTimberwolfMountain1 = false;

    [Name("     Timberwolf Mountain - Disable Territory 2")]
    [Description("Disable Cougar Territory Near Mountaineer's Hut")]
    public bool disableTimberwolfMountain2 = false;

    [Name("     Timberwolf Mountain - Disable Territory 3")]
    [Description("Disable Cougar Territory Near Blackrock Transition")]
    public bool disableTimberwolfMountain3 = false;

    // Constructor to initialize visibility
    public Settings()
    {
        // Initialize visibility settings after construction
        InitializeVisibility();
    }

    private void InitializeVisibility()
    {
        // Set initial visibility based on toggle states
        SetWolfSettingsVisible(showWolfSettings);
        SetBearSettingsVisible(showBearSettings);
        SetCougarSettingsVisible(showCougarSettings);
    }

    protected override void OnChange(FieldInfo field, object oldValue, object newValue)
    {
        if (field.Name == nameof(showWolfSettings))
        {
            SetWolfSettingsVisible((bool)newValue);
        }
        else if (field.Name == nameof(showBearSettings))
        {
            SetBearSettingsVisible((bool)newValue);
        }
        else if (field.Name == nameof(showCougarSettings))
        {
            SetCougarSettingsVisible((bool)newValue);
        }
    }

    private void SetWolfSettingsVisible(bool visible)
    {
        var wolfFields = new string[]
        {
            nameof(regularWolfPercentage), nameof(ashCanyonWolves), nameof(blackRockWolves), nameof(blackRockPrisonWolves), nameof(bleakInletWolves),
            nameof(bleakInletWorkshopWolves), nameof(brokenRailroadWolves), nameof(coastalHighwayWolves), nameof(crumblingHighwayWolves),
            nameof(desolationPointWolves), nameof(desolationPointCaveWolves), nameof(farRangeBranchLineWolves), nameof(forlornMuskegWolves),
            nameof(forsakenAirfieldWolves), nameof(hushedRiverValleyWolves), nameof(keepersPassWolves),
            nameof(miningRegionWolves), nameof(mountainPassWolves), nameof(mountainTownWolves),
            nameof(mysteryLakeWolves), nameof(pleasantValleyWolves), nameof(timberwolfMountainWolves),
            nameof(windingRiverWolves), nameof(disableAllRegularWolves), nameof(disableAllTimberWolves), nameof(disableAllStarvingWolves)
        };

        foreach (string fieldName in wolfFields)
        {
            var field = typeof(Settings).GetField(fieldName);
            if (field != null)
            {
                SetFieldVisible(field, visible);
            }
        }
    }

    private void SetBearSettingsVisible(bool visible)
    {
        var bearFields = new string[]
        {
            nameof(ashCanyonBears), nameof(blackRockBears), nameof(blackRockPrisonBears), nameof(bleakInletBears),
            nameof(brokenRailroadBears), nameof(coastalHighwayBears), nameof(desolationPointBears), nameof(forlornMuskegBears),
            nameof(forsakenAirfieldBears), nameof(hushedRiverValleyBears), nameof(disableAllBears),
            nameof(miningRegionBears), nameof(mountainPassBears), nameof(mountainTownBears),
            nameof(mysteryLakeBears), nameof(pleasantValleyBears), nameof(timberwolfMountainBears)
        };

        foreach (string fieldName in bearFields)
        {
            var field = typeof(Settings).GetField(fieldName);
            if (field != null)
            {
                SetFieldVisible(field, visible);
            }
        }
    }

    private void SetCougarSettingsVisible(bool visible)
    {
        var cougarFields = new string[]
        {
            nameof(disableAllCougars),
            nameof(disableAshCanyon1), nameof(disableAshCanyon2), nameof(disableAshCanyon3),
            nameof(disableBlackrock1), nameof(disableBlackrock2), nameof(disableBlackrock3),
            nameof(disableBleak1), nameof(disableBleak2), nameof(disableBleak3),
            nameof(disableAirfield1), nameof(disableAirfield2), nameof(disableAirfield3),
            nameof(disableRiverValley1), nameof(disableRiverValley2), nameof(disableRiverValley3),
            nameof(disableMountainTown1), nameof(disableMountainTown2), nameof(disableMountainTown3),
            nameof(disableMysteryLake1), nameof(disableMysteryLake2), nameof(disableMysteryLake3),
            nameof(disablePleasantValley1), nameof(disablePleasantValley2), nameof(disablePleasantValley3),
            nameof(disableMountainPass1), nameof(disableMountainPass2), nameof(disableMountainPass3),
            nameof(disableTimberwolfMountain1), nameof(disableTimberwolfMountain2), nameof(disableTimberwolfMountain3)
        };

        foreach (string fieldName in cougarFields)
        {
            var field = typeof(Settings).GetField(fieldName);
            if (field != null)
            {
                SetFieldVisible(field, visible);
            }
        }
    }
}