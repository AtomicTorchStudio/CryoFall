namespace AtomicTorch.CBND.CoreMod.Skills
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class SkillWeaponsEnergy : ProtoSkillWeaponsRanged
    {
        public override string Description =>
            "Using energy weapons more often allows you to better understand how they work, thus improving the overall efficiency.";

        public override double ExperienceAddedOnKillPerMaxEnemyHealthMultiplier => 0.2;

        public override double ExperienceAddedPerDamageDoneMultiplier => 0.75;

        /// <summary>
        /// Energy is almost free so to balance it the game won't reward shooting
        /// but provide increased ExperienceAddedPerDamageDoneMultiplier.
        /// </summary>
        public override double ExperienceAddedPerShot => 0;

        public override double ExperienceToLearningPointsConversionMultiplier => 1.0;

        public override string Name => "Energy weapons";

        public override StatName StatNameDamageBonusMultiplier
            => StatName.WeaponEnergyDamageBonusMultiplier;

        public override StatName StatNameDegrationRateMultiplier
            => StatName.WeaponEnergyDegradationRateMultiplier;

        // no reloading stat
        public override StatName? StatNameReloadingSpeedMultiplier
            => null;

        public override StatName StatNameSpecialEffectChanceMultiplier
            => StatName.WeaponEnergySpecialEffectChanceMultiplier;

        public static double SharedGetRequiredEnergyAmount(ICharacter character, double energyUsePerShot)
        {
            var multiplier = character.SharedGetFinalStatMultiplier(
                StatName.WeaponEnergyWeaponEnergyConsumptionMultiplier);

            return energyUsePerShot * multiplier;
        }

        protected override void PrepareProtoWeaponsSkillRanged(SkillConfig config)
        {
            config.AddStatEffect(
                StatName.WeaponEnergyWeaponEnergyConsumptionMultiplier,
                formulaPercentBonus: level => -level * 2);

            var statNameDamageBonus = this.StatNameDamageBonusMultiplier;
            var statNameReloadingSpeed = this.StatNameReloadingSpeedMultiplier;
            var statNameDegradationRate = this.StatNameDegrationRateMultiplier;

            config.AddStatEffect(
                statNameDamageBonus,
                level: 10,
                percentBonus: 2);

            config.AddStatEffect(
                statNameDamageBonus,
                level: 20,
                percentBonus: 3);

            if (statNameReloadingSpeed.HasValue)
            {
                config.AddStatEffect(
                    statNameReloadingSpeed.Value,
                    formulaPercentBonus: level => -level * 2);
            }

            config.AddStatEffect(
                statNameDegradationRate,
                formulaPercentBonus: level => -level * 2);
        }
    }
}