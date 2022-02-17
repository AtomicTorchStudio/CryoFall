namespace AtomicTorch.CBND.CoreMod.Items
{
    using JetBrains.Annotations;

    /// <summary>
    /// This is a enum of all available item skins.
    /// It's automatically mapped to the skin classes.
    /// If there is a skin class but no mapping for it in this enum, a error is reported (and vice versa).
    /// 
    /// Please note: do not modify this enum. If you wish to add a skin in your mod, just use "None" as skin ID
    /// and add this property override:
    /// public override bool IsModSkin => true;
    /// </summary>
    [UsedImplicitly]
    // @formatter:off
    public enum SkinId : ushort
    {
        None = 0,

        // weapons
        CrossbowPrimitive = 100,
        CrossbowModern = 101,
        CrossbowEmperor = 102,

        FlintlockPistolPrimitive = 200,
        FlintlockPistolBuccaneer = 201,
        FlintlockPistolEmperor = 202,

        MusketPrimitive = 300,
        MusketRedwood = 301,
        MusketEmperor = 302,

        Revolver8mmSheriff = 400,
        Revolver8mmNoble = 401,
        Revolver8mmPantheon = 402,

        MachinePistolMarauder = 500,
        MachinePistolHardened = 501,
        MachinePistolLumen = 502,

        ShotgunDoublebarreledMarauder = 600,
        ShotgunDoublebarreledEmperor = 601,
        ShotgunDoublebarreledGraphene = 602,

        RifleBoltActionMarauder = 700,
        RifleBoltActionHunter = 701,
        RifleBoltActionNoble = 702,

        Handgun10mmItalian = 800,
        Handgun10mmUrbanCamo = 801,
        Handgun10mmNoble = 802,

        SubMachinegun10mmForestCamo = 900,
        SubMachinegun10mmRoots = 901,
        SubMachinegun10mmAquamarine = 902,

        Rifle10mmMarauder = 1000,
        Rifle10mmUrbanCamo = 1001,
        Rifle10mmPhoenix = 1002,

        ShotgunMilitaryUrbanCamo = 1100,
        ShotgunMilitaryPhoenix = 1101,
        ShotgunMilitaryDesertCamo = 1102,

        GrenadeLauncherForestCamo = 1200,
        GrenadeLauncherRedMarble = 1201,
        GrenadeLauncherDesertCamo = 1202,

        SteppenHawkWasp = 1300,
        SteppenHawkPantheon = 1301,
        SteppenHawkNecromancer = 1302,

        Machinegun300ForestCamo = 1400,
        Machinegun300Lightning = 1401,
        Machinegun300DesertCamo = 1402,
        Machinegun300CryoFall = 1403,

        Rifle300Aquamarine = 1500,
        Rifle300UrbanCamo = 1501,
        Rifle300Roots = 1502,
        Rifle300CryoFall = 1503,

        GrenadeLauncherMultiMagma = 1600,
        GrenadeLauncherMultiUrbanCamo = 1601,
        GrenadeLauncherMultiForestCamo = 1602,

        LaserPistolParadox = 1700,
        LaserPistolRedLine = 1701,
        LaserPistolZenith = 1702,

        PlasmaRifleLightning = 1800,
        PlasmaRifleParadox = 1801,
        PlasmaRifleRedMarble = 1802,

        LaserRifleParadox = 1900,
        LaserRiflePrototype = 1901,
        LaserRifleZenith = 1902,

        SwarmLauncherHive = 2000,
        SwarmLauncherScar = 2001,

        ToxinProliferatorHive = 2100,
        ToxinProliferatorHypnotic = 2101,

        RapierLaserEmerald = 5000,
        RapierLaserRuby = 5001,
        RapierLaserSapphire = 5002,
        RapierLaserTopaz = 5003,
        RapierLaserTourmaline = 5004,

        // armor
        WoodArmorVooDoo = 10000,

        WoodHelmetCork = 10100,
        WoodHelmetVoodoo = 10101,

        BraidedArmorLamellar = 10200,

        BraidedHelmetStraw = 10300,
        BraidedHelmetSki = 10301,

        BoneArmorNecromancer = 10400,

        BoneHelmetNecromancer = 10500,
        BoneHelmetShaman = 10501,

        LeatherArmorBiker = 10600,

        LeatherHelmetAviator = 10700,
        LeatherHelmetPirate = 10701,
        LeatherHelmetCowboy = 10702,

        FurArmorFancy = 10800,

        FurHelmetCossak = 10900,
        FurHelmetUshanka = 10901,

        QuiltedArmorPuffer = 11000,

        QuiltedHelmetNomad = 11001,
        QuiltedHelmetRevolutionary = 11002,

        MetalArmorKnight = 11100,

        MetalHelmetKnight = 11200,
        MetalHelmetSkull = 11201,

        HazmatSuitContagion = 11300,

        MilitaryArmorCommando = 11400,

        MilitaryHelmetCommando = 11500,
        MilitaryHelmetExecutioner = 11501,

        ApartSuitExplorer = 11600,

        // IMPORTANT: the following section of assault armor and helmets has incorrect identifiers.
        // Please excercise caution when adding new skins.
        AssaultArmorSnake = 11700,
        AssaultHelmetElite = 11701, // incorrect ID, it must have been a part of the helmets section, but was placed into armor section
        AssaultArmorCryoFall = 11702,

        AssaultHelmetMercenary = 11800,
        AssaultHelmetCryoFall = 11801,


        PragmiumSuitAlien = 11900,

        SuperHeavySuitIridium = 12000,

        ClothHelmetHalloween = 12100,
        ClothHelmetHolidays = 12101
    }
}