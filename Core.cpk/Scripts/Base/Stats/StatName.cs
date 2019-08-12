namespace AtomicTorch.CBND.CoreMod.Stats
{
    using System.ComponentModel;
    using AtomicTorch.CBND.CoreMod.Skills;

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

        AttackFinalDamageMultiplier,

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
        [RelatedToSkill(typeof(SkillSearching))]
        SearchingExtraLoot,

        [Description("Hunting extra loot chance")]
        [RelatedToSkill(typeof(SkillHunting))]
        HuntingExtraLoot,

        [Description("Farming tasks speed")]
        FarmingTasksSpeed,

        [Description("Plants growth speed")]
        [RelatedToSkill(typeof(SkillFarming))]
        FarmingPlantGrowSpeed,

        [Description("Searching speed")]
        SearchingSpeed,

        [Description("Damage bonus")]
        [RelatedToSkill(typeof(SkillWeaponsConventional))]
        WeaponConventionalDamageBonusMultiplier,

        [Description("Damage bonus")]
        [RelatedToSkill(typeof(SkillWeaponsMelee))]
        WeaponMeleeDamageBonusMultiplier,

        [Description("Damage bonus")]
        [RelatedToSkill(typeof(SkillWeaponsEnergy))]
        WeaponEnergyDamageBonusMultiplier,

        [Description("Damage bonus")]
        [RelatedToSkill(typeof(SkillWeaponsHeavy))]
        WeaponHeavyDamageBonusMultiplier,

        [Description("Damage bonus")]
        //[RelatedToSkill(typeof(SkillWeaponsExotic))]
        WeaponExoticDamageBonusMultiplier,

        [Description("Special effect chance")]
        [RelatedToSkill(typeof(SkillWeaponsConventional))]
        WeaponConventionalSpecialEffectChanceMultiplier,

        [Description("Special effect chance")]
        [RelatedToSkill(typeof(SkillWeaponsMelee))]
        WeaponMeleeSpecialEffectChanceMultiplier,

        [Description("Special effect chance")]
        [RelatedToSkill(typeof(SkillWeaponsEnergy))]
        WeaponEnergySpecialEffectChanceMultiplier,

        [Description("Special effect chance")]
        [RelatedToSkill(typeof(SkillWeaponsHeavy))]
        WeaponHeavySpecialEffectChanceMultiplier,

        [Description("Special effect chance")]
        //[RelatedToSkill(typeof(SkillWeaponsExotic))]
        WeaponExoticSpecialEffectChanceMultiplier,

        [Description("Degradation rate")]
        [RelatedToSkill(typeof(SkillWeaponsConventional))]
        WeaponConventionalDegradationRateMultiplier,

        [Description("Degradation rate")]
        [RelatedToSkill(typeof(SkillWeaponsMelee))]
        WeaponMeleeDegradationRateMultiplier,

        [Description("Degradation rate")]
        [RelatedToSkill(typeof(SkillWeaponsEnergy))]
        WeaponEnergyDegradationRateMultiplier,

        [Description("Degradation rate")]
        [RelatedToSkill(typeof(SkillWeaponsHeavy))]
        WeaponHeavyDegradationRateMultiplier,

        [Description("Degradation rate")]
        //[RelatedToSkill(typeof(SkillWeaponsExotic))]
        WeaponExoticDegradationRateMultiplier,

        [Description("Reloading time")]
        [RelatedToSkill(typeof(SkillWeaponsConventional))]
        WeaponConventionalReloadingSpeedMultiplier,

        [Description("Reloading time")]
        [RelatedToSkill(typeof(SkillWeaponsMelee))]
        WeaponMeleeReloadingSpeedMultiplier,

        [Description("Energy consumption")]
        [RelatedToSkill(typeof(SkillWeaponsEnergy))]
        WeaponEnergyWeaponEnergyConsumptionMultiplier,

        [Description("Reloading time")]
        [RelatedToSkill(typeof(SkillWeaponsHeavy))]
        WeaponHeavyReloadingSpeedMultiplier,

        [Description("Reloading time")]
        //[RelatedToSkill(typeof(SkillWeaponsExotic))]
        WeaponExoticReloadingSpeedMultiplier,

        [Description("Crafting speed")]
        CraftingSpeed,

        [Description("Explosive planting time")]
        ItemExplosivePlantingSpeedMultiplier,

        [RelatedToSkill(typeof(SkillHunting))]
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

        [Description("Radiation poisoning damage")]
        RadiationPoisoningEffectMultiplier,

        [Description("Radiation accumulation rate")]
        RadiationPoisoningIncreaseRateMultiplier,

        [Description("Toxins accumulation rate")]
        ToxinsIncreaseRateMultiplier,

        [Description("Bleeding")]
        BleedingIncreaseRateMultiplier,

        [Description("Heat increase rate")]
        HeatIncreaseRateMultiplier,

        [Description("Heat damage")]
        HeatEffectMultiplier,

        [Description("Pain")]
        PainIncreaseRateMultiplier,

        [Description("Psi damage")]
        PsiEffectMultiplier,

        [Description("Dazed")]
        DazedIncreaseRateMultiplier,

        [Description("Maximum number of land claims")]
        LandClaimsMaxNumber,

        /// <summary>
        /// This is a vanity stat. It is not used in any calculations and simply exists to be displayed in tooltip for status
        /// effects that damage the player.
        /// </summary>
        [Description("Continuous damage")]
        [StatNameHiddenValue]
        VanityContinuousDamage,

        /// <summary>
        /// This is a vanity stat. It is not used in any calculations and simply exists to be displayed in tooltip for status
        /// effects that damage the player.
        /// </summary>
        [Description("Can't eat or drink")]
        [StatNameHiddenValue]
        VanityCantEatOrDrink,

        [Description("Learning points gain")]
        LearningsPointsGainMultiplier,

        [Description("Tinker table effectiveness")]
        TinkerTableBonus,

        [Description("Crafting queue slots")]
        CraftingQueueMaxSlotsCount
    }
}