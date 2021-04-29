namespace AtomicTorch.CBND.CoreMod.Skills
{
    using AtomicTorch.CBND.CoreMod.Stats;

    public class SkillWeaponsExotic : ProtoSkillWeaponsRanged
    {
        public override string Description =>
            "Utilizing exotic weapons in battle provides you with more insight into their secrets, giving you a better understanding on how to maximize their potential.";

        public override double ExperienceAddedOnKillPerMaxEnemyHealthMultiplier => 0.25;

        public override double ExperienceAddedPerDamageDoneMultiplier => 0.75;

        /// <summary>
        /// This is intended to reward experience per ammo expended. Basically resource->exp conversion.
        /// </summary>
        public override double ExperienceAddedPerShot => 5;

        public override double ExperienceToLearningPointsConversionMultiplier => 1.0;

        public override string Name => "Exotic weapons";

        public override StatName StatNameDamageBonusMultiplier
            => StatName.WeaponExoticDamageBonusMultiplier;

        public override StatName StatNameDegrationRateMultiplier
            => StatName.WeaponExoticDegradationRateMultiplier;

        public override StatName? StatNameReloadingSpeedMultiplier
            => StatName.WeaponExoticReloadingSpeedMultiplier;

        public override StatName StatNameSpecialEffectChanceMultiplier
            => StatName.WeaponExoticSpecialEffectChanceMultiplier;

        protected override void PrepareProtoWeaponsSkillRanged(SkillConfig config)
        {
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