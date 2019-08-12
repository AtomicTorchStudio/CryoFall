namespace AtomicTorch.CBND.CoreMod.Skills
{
    using AtomicTorch.CBND.CoreMod.Stats;

    public class SkillWeaponsConventional : ProtoSkillWeaponsRanged
    {
        public override string Description =>
            "Gaining more experience with all conventional firearms makes you accustomed to all of their nuances and enables you to use any of them more effectively.";

        public override double ExperienceAddedOnKillPerMaxEnemyHealthMultiplier => 0.2;

        public override double ExperienceAddedPerDamageDoneMultiplier => 0.5;

        public override double ExperienceToLearningPointsConversionMultiplier => 1.0;

        public override string Name => "Conventional weapons";

        public override StatName StatNameDamageBonusMultiplier
            => StatName.WeaponConventionalDamageBonusMultiplier;

        public override StatName StatNameDegrationRateMultiplier
            => StatName.WeaponConventionalDegradationRateMultiplier;

        public override StatName? StatNameReloadingSpeedMultiplier
            => StatName.WeaponConventionalReloadingSpeedMultiplier;

        public override StatName StatNameSpecialEffectChanceMultiplier
            => StatName.WeaponConventionalSpecialEffectChanceMultiplier;
    }
}