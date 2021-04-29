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
        [StatBuff]
        HealthMax,

        [Description("Max stamina")]
        [StatBuff]
        StaminaMax,

        [Description("Max food")]
        [StatBuff]
        FoodMax,

        [Description("Max water")]
        [StatBuff]
        WaterMax,

        [Description("Move speed")]
        [StatBuff]
        MoveSpeed,

        [Description("Run speed")]
        [StatBuff]
        MoveSpeedRunMultiplier,

        [Obsolete("Currently not used, but could be implemented if needed. Localization is already there.")]
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
        [StatDebuff]
        RunningStaminaConsumptionPerSecond,

        [Description("Stamina regeneration")]
        [StatBuff]
        StaminaRegenerationPerSecond,

        [Description("Health regeneration")]
        [StatBuff]
        HealthRegenerationPerSecond,

        [Description("Mining speed")]
        [StatBuff]
        MiningSpeed,

        [Description("Woodcutting speed")]
        [StatBuff]
        WoodcuttingSpeed,

        [Description("Foraging speed")]
        [StatBuff]
        ForagingSpeed,

        [Description("Building/deconstruction speed")]
        [StatBuff]
        BuildingSpeed,

        [Description("Scavenging extra loot chance")]
        [RelatedToSkill(typeof(SkillSearching))]
        [StatBuff]
        SearchingExtraLoot,

        [Description("Hunting extra loot chance")]
        [RelatedToSkill(typeof(SkillHunting))]
        [StatBuff]
        HuntingExtraLoot,

        [Description("Farming tasks speed")]
        [StatBuff]
        FarmingTasksSpeed,

        [Description("Plants growth speed")]
        [RelatedToSkill(typeof(SkillFarming))]
        [StatBuff]
        FarmingPlantGrowSpeed,

        [Description("Searching speed")]
        [StatBuff]
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
        [RelatedToSkill(typeof(SkillWeaponsExotic))]
        WeaponExoticDamageBonusMultiplier,

        [Description("Special effect chance")]
        [RelatedToSkill(typeof(SkillWeaponsExotic))]
        WeaponExoticSpecialEffectChanceMultiplier,

        [Description("Degradation rate")]
        [RelatedToSkill(typeof(SkillWeaponsExotic))]
        WeaponExoticDegradationRateMultiplier,

        [Description("Reloading time")]
        [RelatedToSkill(typeof(SkillWeaponsExotic))]
        WeaponExoticReloadingSpeedMultiplier,

        [Description("Crafting speed")]
        [StatBuff]
        CraftingSpeed,

        [Description("Explosive planting speed")]
        [StatBuff]
        ItemExplosivePlantingSpeedMultiplier,

        [RelatedToSkill(typeof(SkillHunting))]
        [Description("Looting speed")]
        [StatBuff]
        HuntingLootingSpeed,

        [Description("Implant degradation from damage taken")]
        [StatDebuff]
        ImplantDegradationFromDamageMultiplier,

        [Description("Installed implant degradation speed")]
        [StatDebuff]
        ImplantDegradationSpeedMultiplier,

        // Speed of getting hungry
        [Description("Hunger rate")]
        [StatDebuff]
        HungerRate,

        // Speed of getting thirsty
        [Description("Thirst rate")]
        [StatDebuff]
        ThirstRate,

        [Description("Energy charge regeneration (per minute)")]
        [StatBuff]
        EnergyChargeRegenerationPerMinute,

        [Description("Radiation poisoning damage")]
        [StatDebuff]
        RadiationPoisoningEffectMultiplier,

        [Description("Radiation accumulation rate")]
        [StatDebuff]
        RadiationPoisoningIncreaseRateMultiplier,

        [Description("Toxins accumulation rate")]
        [StatDebuff]
        ToxinsIncreaseRateMultiplier,

        [Description("Bleeding")]
        [StatDebuff]
        BleedingIncreaseRateMultiplier,

        [Obsolete("Currently not used, but could be implemented if needed. Localization is already there.")]
        [Description("Heat increase rate")]
        [StatDebuff]
        HeatIncreaseRateMultiplier,

        [Description("Heat damage")]
        [StatDebuff]
        HeatEffectMultiplier,

        [Description("Pain")]
        [StatDebuff]
        PainIncreaseRateMultiplier,

        /// <summary>
        /// Please note: this is damage received by psi status effect, not a damage modifier for psi damage dealt.
        /// </summary>
        [Description("Psi damage")]
        [StatDebuff]
        PsiEffectMultiplier,

        [Description("Dazed")]
        [StatDebuff]
        DazedIncreaseRateMultiplier,

        [Description("Maximum number of land claims")]
        [StatBuff]
        LandClaimsMaxNumber,

        /// <summary>
        /// This is a vanity stat. It is not used in any calculations and simply exists to be displayed in tooltip for status
        /// effects that damage the player.
        /// </summary>
        [Description("Continuous damage")]
        [StatDebuff]
        [StatValueHidden]
        VanityContinuousDamage,

        /// <summary>
        /// This is a vanity stat. It is not used in any calculations and simply exists to be displayed in tooltip for status
        /// effects that damage the player.
        /// </summary>
        [Description("Can't eat or drink")]
        [StatDebuff]
        [StatValueHidden]
        VanityCantEatOrDrink,

        /// <summary>
        /// This is a vanity stat. It is not used in any calculations and simply exists to be displayed in tooltip for status
        /// effects that damage the player.
        /// </summary>
        [Description("Low light vision")]
        [StatBuff]
        [StatValueHidden]
        VanityLowLightVision,

        [Description("Learning points gain")]
        [StatBuff]
        LearningsPointsGainMultiplier,

        [Description("Skills experience gain")]
        [StatBuff]
        SkillsExperienceGainMultiplier,

        [Description("Tinker table effectiveness")]
        [StatBuff]
        TinkerTableBonus,

        [Description("Crafting queue slots")]
        [StatBuff]
        CraftingQueueMaxSlotsCount,

        [Description("Medicine toxicity")]
        [StatDebuff]
        MedicineToxicityMultiplier,

        [Description("Fishing knowledge level")]
        [StatBuff]
        FishingKnowledgeLevel,

        [Description("Fishing success")]
        [StatBuff]
        FishingSuccess,

        [Description("Fuel consumption")]
        [RelatedToSkill(typeof(SkillVehicles))]
        [StatDebuff]
        VehicleFuelConsumptionRate,

        [Description("Ability to eat spoiled food")]
        [StatBuff]
        [StatValueHidden]
        PerkEatSpoiledFood,

        [Description("Ability to overeat without consequences")]
        [StatBuff]
        [StatValueHidden]
        PerkOvereatWithoutConsequences,

        [Description("Cannot run")]
        [StatDebuff]
        [StatValueHidden]
        PerkCannotRun,

        [Description("Cannot attack")]
        [StatDebuff]
        [StatValueHidden]
        PerkCannotAttack,

        [Description("Cannot use medical items that have a cooldown")]
        [StatDebuff]
        [StatValueHidden]
        PerkCannotUseMedicalItems,

        /// <summary>
        /// Used in StatusEffectBrokenLeg to determine whether the status effect could be added to the player.
        /// </summary>
        [Description("Cannot break bones")]
        [StatBuff]
        [StatValueHidden]
        PerkCannotBreakBones
    }
}