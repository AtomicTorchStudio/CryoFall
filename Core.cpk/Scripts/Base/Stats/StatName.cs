namespace AtomicTorch.CBND.CoreMod.Stats
{
    using System;
    using System.ComponentModel;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.GameApi;

    [RemoteEnum]
    public enum StatName : ushort
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

        DefenseExplosion,

        DefenseHeat,

        DefenseCold,

        DefenseChemical,

        DefenseRadiation,

        DefensePsi,

        DamageProportionImpact,

        DamageProportionKinetic,

        DamageProportionExplosion,

        DamageProportionHeat,

        DamageProportionCold,

        DamageProportionChemical,

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
        [RelatedToSkill(typeof(SkillWeaponsMelee))]
        WeaponMeleeDamageBonusMultiplier,

        [Description("Special effect chance")]
        [RelatedToSkill(typeof(SkillWeaponsMelee))]
        WeaponMeleeSpecialEffectChanceMultiplier,

        [Description("Degradation rate")]
        [RelatedToSkill(typeof(SkillWeaponsMelee))]
        WeaponMeleeDegradationRateMultiplier,

        [Description("Reloading time")]
        [RelatedToSkill(typeof(SkillWeaponsMelee))]
        WeaponMeleeReloadingSpeedMultiplier,

        [Description("Damage bonus")]
        [RelatedToSkill(typeof(SkillWeaponsConventional))]
        WeaponConventionalDamageBonusMultiplier,

        [Description("Special effect chance")]
        [RelatedToSkill(typeof(SkillWeaponsConventional))]
        WeaponConventionalSpecialEffectChanceMultiplier,

        [Description("Degradation rate")]
        [RelatedToSkill(typeof(SkillWeaponsConventional))]
        WeaponConventionalDegradationRateMultiplier,

        [Description("Reloading time")]
        [RelatedToSkill(typeof(SkillWeaponsConventional))]
        WeaponConventionalReloadingSpeedMultiplier,

        [Description("Damage bonus")]
        [RelatedToSkill(typeof(SkillWeaponsHeavy))]
        WeaponHeavyDamageBonusMultiplier,

        [Description("Special effect chance")]
        [RelatedToSkill(typeof(SkillWeaponsHeavy))]
        WeaponHeavySpecialEffectChanceMultiplier,

        [Description("Degradation rate")]
        [RelatedToSkill(typeof(SkillWeaponsHeavy))]
        WeaponHeavyDegradationRateMultiplier,

        [Description("Reloading time")]
        [RelatedToSkill(typeof(SkillWeaponsHeavy))]
        WeaponHeavyReloadingSpeedMultiplier,

        [Description("Damage bonus")]
        [RelatedToSkill(typeof(SkillWeaponsEnergy))]
        WeaponEnergyDamageBonusMultiplier,

        [Description("Special effect chance")]
        [RelatedToSkill(typeof(SkillWeaponsEnergy))]
        WeaponEnergySpecialEffectChanceMultiplier,

        [Description("Degradation rate")]
        [RelatedToSkill(typeof(SkillWeaponsEnergy))]
        WeaponEnergyDegradationRateMultiplier,

        [Description("Energy consumption")]
        [RelatedToSkill(typeof(SkillWeaponsEnergy))]
        WeaponEnergyWeaponEnergyConsumptionMultiplier,

        [Description("Weapon damage bonus")]
        [RelatedToSkill(typeof(SkillVehicles))]
        WeaponVehicleDamageBonusMultiplier,

        [Description("Weapon degradation rate")]
        [RelatedToSkill(typeof(SkillVehicles))]
        WeaponVehicleDegrationRateMultiplier,

        [Description("Weapon reloading time")]
        [RelatedToSkill(typeof(SkillVehicles))]
        WeaponVehicleReloadingSpeedMultiplier,

        [Description("Weapon special effect chance")]
        [RelatedToSkill(typeof(SkillVehicles))]
        WeaponVehicleSpecialEffectChanceMultiplier,

        [Description("Damage bonus")]
        //[RelatedToSkill(typeof(SkillWeaponsExotic))]
        WeaponExoticDamageBonusMultiplier,

        [Description("Special effect chance")]
        //[RelatedToSkill(typeof(SkillWeaponsExotic))]
        WeaponExoticSpecialEffectChanceMultiplier,

        [Description("Degradation rate")]
        //[RelatedToSkill(typeof(SkillWeaponsExotic))]
        WeaponExoticDegradationRateMultiplier,

        [Obsolete("Currently not used and also the name is not matching the description")]
        [Description("Reloading time")]
        //[RelatedToSkill(typeof(SkillWeaponsExotic))]
        WeaponExoticReloadingSpeedMultiplier,

        [Description("Crafting speed")]
        CraftingSpeed,

        [Description("Explosive planting speed")]
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

        [Description("Radiation poisoning damage")]
        RadiationPoisoningEffectMultiplier,

        [Description("Radiation accumulation rate")]
        RadiationPoisoningIncreaseRateMultiplier,

        [Description("Toxins accumulation rate")]
        ToxinsIncreaseRateMultiplier,

        [Description("Bleeding")]
        BleedingIncreaseRateMultiplier,

        [Obsolete("Currently not used, but could be implemented if needed. Localization is already there.")]
        [Description("Heat increase rate")]
        HeatIncreaseRateMultiplier,

        [Description("Heat damage")]
        HeatEffectMultiplier,

        [Description("Pain")]
        PainIncreaseRateMultiplier,

        /// <summary>
        /// Please note: this is damage received by psi status effect, not a damage modifier for psi damage dealt.
        /// </summary>
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
        [StatValueHidden]
        VanityContinuousDamage,

        /// <summary>
        /// This is a vanity stat. It is not used in any calculations and simply exists to be displayed in tooltip for status
        /// effects that damage the player.
        /// </summary>
        [Description("Can't eat or drink")]
        [StatValueHidden]
        VanityCantEatOrDrink,

        [Description("Learning points gain")]
        LearningsPointsGainMultiplier,

        [Description("Skills experience gain")]
        SkillsExperienceGainMultiplier,

        [Description("Tinker table effectiveness")]
        TinkerTableBonus,

        [Description("Crafting queue slots")]
        CraftingQueueMaxSlotsCount,

        [Description("Medicine toxicity")]
        MedicineToxicityMultiplier,

        [Description("Fishing knowledge level")]
        FishingKnowledgeLevel,

        [Description("Fishing success")]
        FishingSuccess,

        [Description("Fuel consumption")]
        [RelatedToSkill(typeof(SkillVehicles))]
        VehicleFuelConsumptionRate,

        [Description("Ability to eat spoiled food")]
        [StatValueHidden]
        PerkEatSpoiledFood,

        [Description("Ability to overeat without consequences")]
        [StatValueHidden]
        PerkOvereatWithoutConsequences,

        [Description("Cannot run")]
        [StatValueHidden]
        PerkCannotRun,

        [Description("Cannot attack")]
        [StatValueHidden]
        PerkCannotAttack,

        [Description("Cannot use medical items that have a cooldown")]
        [StatValueHidden]
        PerkCannotUseMedicalItems,

        /// <summary>
        /// Used in StatusEffectBrokenLeg to determine whether the status effect could be added to the player.
        /// </summary>
        [Description("Cannot break bones")]
        [StatValueHidden]
        PerkCannotBreakBones
    }
}