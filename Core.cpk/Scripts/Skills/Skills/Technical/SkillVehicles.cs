namespace AtomicTorch.CBND.CoreMod.Skills
{
    using AtomicTorch.CBND.CoreMod.Stats;

    public class SkillVehicles : ProtoSkillWeaponsRanged
    {
        public const double ExperienceForDrivingVehiclePerSecond = 1;

        public override string Description =>
            "Ability to pilot various vehicles and use their equipment correctly and effectively. Applies to all vehicle types.";

        public override double ExperienceAddedOnKillPerMaxEnemyHealthMultiplier => 0.2;

        public override double ExperienceAddedPerDamageDoneMultiplier => 0.5;

        /// <summary>
        /// This is intended to reward experience per ammo expended. Basically resource->exp conversion.
        /// </summary>
        public override double ExperienceAddedPerShot => 15;

        public override double ExperienceToLearningPointsConversionMultiplier => 1.0;

        public override string Name => "Vehicles";

        public override StatName StatNameDamageBonusMultiplier
            => StatName.WeaponVehicleDamageBonusMultiplier;

        public override StatName StatNameDegrationRateMultiplier
            => StatName.WeaponVehicleDegrationRateMultiplier;

        public override StatName? StatNameReloadingSpeedMultiplier
            => StatName.WeaponVehicleReloadingSpeedMultiplier;

        public override StatName StatNameSpecialEffectChanceMultiplier
            => StatName.WeaponVehicleSpecialEffectChanceMultiplier;

        protected override void PrepareProtoWeaponsSkillRanged(SkillConfig config)
        {
            config.Category = GetCategory<SkillCategoryTechnical>();

            var statNameReloadingSpeed = this.StatNameReloadingSpeedMultiplier;
            if (statNameReloadingSpeed.HasValue)
            {
                config.AddStatEffect(
                    statNameReloadingSpeed.Value,
                    formulaPercentBonus: level => -level * 2);
            }

            config.AddStatEffect(
                StatName.VehicleFuelConsumptionRate,
                formulaPercentBonus: level => -level,
                displayTotalValue: true);
        }
    }
}