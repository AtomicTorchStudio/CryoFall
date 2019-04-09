namespace AtomicTorch.CBND.CoreMod.Stats
{
    using System.ComponentModel;

    public enum StatName
    {
        [Description("Max health")]
        HealthMax,

        [Description("Max stamina")]
        StaminaMax,

        [Description("Max food")]
        FoodMax,

        [Description("Max water")]
        WaterMax,

        [Description("Move speed")]
        MoveSpeed,

        [Description("Run speed")]
        MoveSpeedRunMultiplier,

        AttackFinalDamageMultiplier,

        [Description("Additional damage")]
        DamageAdd,

        DefenseImpact,

        DefenseKinetic,

        DefenseHeat,

        DefenseCold,

        DefenseChemical,

        DefenseElectrical,

        DefenseRadiation,

        DefensePsi,

        DamageProportionImpact,

        DamageProportionKinetic,

        DamageProportionHeat,

        DamageProportionCold,

        DamageProportionChemical,

        DamageProportionElectrical,

        DamageProportionRadiation,

        DamageProportionPsi,

        AttackRangeMax,

        AttackArmorPiercingValue,

        AttackArmorPiercingMultiplier,

        [Description("Stamina depletion when running")]
        RunningStaminaConsumptionPerSecond,

        [Description("Stamina regeneration")]
        StaminaRegenerationPerSecond,

        [Description("Health regeneration")]
        HealthRegenerationPerSecond,

        [Description("Mining speed")]
        MiningSpeed,

        [Description("Woodcutting speed")]
        WoodcuttingSpeed,

        [Description("Foraging speed")]
        ForagingSpeed,

        [Description("Building/deconstruction speed")]
        BuildingSpeed,

        [Description("Scavenging extra loot chance")]
        SearchingExtraLoot,

        [Description("Hunting extra loot chance")]
        HuntingExtraLoot,

        [Description("Farming tasks speed")]
        FarmingTasksSpeed,

        [Description("Plants growth speed")]
        FarmingPlantGrowSpeed,

        [Description("Damage bonus")]
        WeaponConventionalDamageBonusMultiplier,

        [Description("Damage bonus")]
        WeaponMeleeDamageBonusMultiplier,

        [Description("Damage bonus")]
        WeaponEnergyDamageBonusMultiplier,

        [Description("Damage bonus")]
        WeaponHeavyDamageBonusMultiplier,

        [Description("Damage bonus")]
        WeaponExoticDamageBonusMultiplier,

        [Description("Special effect chance")]
        WeaponConventionalSpecialEffectChanceMultiplier,

        [Description("Special effect chance")]
        WeaponMeleeSpecialEffectChanceMultiplier,

        [Description("Special effect chance")]
        WeaponEnergySpecialEffectChanceMultiplier,

        [Description("Special effect chance")]
        WeaponHeavySpecialEffectChanceMultiplier,

        [Description("Special effect chance")]
        WeaponExoticSpecialEffectChanceMultiplier,

        [Description("Degradation rate")]
        WeaponConventionalDegradationRateMultiplier,

        [Description("Degradation rate")]
        WeaponMeleeDegradationRateMultiplier,

        [Description("Degradation rate")]
        WeaponEnergyDegradationRateMultiplier,

        [Description("Degradation rate")]
        WeaponHeavyDegradationRateMultiplier,

        [Description("Degradation rate")]
        WeaponExoticDegradationRateMultiplier,

        [Description("Reloading time")]
        WeaponConventionalReloadingSpeedMultiplier,

        [Description("Reloading time")]
        WeaponMeleeReloadingSpeedMultiplier,

        [Description("Energy consumption")]
        WeaponEnergyWeaponEnergyConsumptionMultiplier,

        [Description("Reloading time")]
        WeaponHeavyReloadingSpeedMultiplier,

        [Description("Reloading time")]
        WeaponExoticReloadingSpeedMultiplier,

        [Description("LP retained after death")]
        LearningPointsRetainedAfterDeath,

        [Description("Crafting speed")]
        CraftingSpeed,

        [Description("Explosive planting time")]
        ItemExplosivePlantingSpeedMultiplier,

        [Description("Looting speed")]
        HuntingLootingSpeed,

        [Description("Implant degradation from damage taken")]
        ImplantDegradationFromDamageMultiplier,

        [Description("Installed implant degradation speed")]
        ImplantDegradationSpeedMultiplier,

        [Description("Food consumption speed")]
        FoodConsumptionSpeedMultiplier,

        [Description("Water consumption speed")]
        WaterConsumptionSpeedMultiplier,

        [Description("Energy charge regeneration (per minute)")]
        EnergyChargeRegenerationPerMinute,

        [Description("Ability to eat spoiled food")]
        PerkEatSpoiledFood,

        [Description("Searching speed")]
        SearchingSpeed
    }
}