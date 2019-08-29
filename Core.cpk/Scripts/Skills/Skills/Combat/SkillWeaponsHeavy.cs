namespace AtomicTorch.CBND.CoreMod.Skills
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class SkillWeaponsHeavy : ProtoSkillWeaponsRanged
    {
        private const double ExperienceAddWhenExplosivePlanted = 500.0;

        public override string Description =>
            "Heavy weaponry usage trains you to operate it with greater skill, providing additional means to bring on the hurt.";

        public override double ExperienceAddedOnKillPerMaxEnemyHealthMultiplier => 0.2;

        public override double ExperienceAddedPerDamageDoneMultiplier => 0.5;

        public override double ExperienceToLearningPointsConversionMultiplier => 1.0;

        public override string Name => "Heavy weapons";

        public override StatName StatNameDamageBonusMultiplier
            => StatName.WeaponHeavyDamageBonusMultiplier;

        public override StatName StatNameDegrationRateMultiplier
            => StatName.WeaponHeavyDegradationRateMultiplier;

        public override StatName? StatNameReloadingSpeedMultiplier
            => StatName.WeaponHeavyReloadingSpeedMultiplier;

        public override StatName StatNameSpecialEffectChanceMultiplier
            => StatName.WeaponHeavySpecialEffectChanceMultiplier;

        public static void ServerAddItemExplosivePlantingExperience(
            ICharacter character,
            double experienceMultiplier)
        {
            // if multiplier is zero or negative - do not add anythying.
            if (experienceMultiplier <= 0)
            {
                return;
            }

            var skills = character.SharedGetSkills();
            skills.ServerAddSkillExperience<SkillWeaponsHeavy>(
                experienceMultiplier * ExperienceAddWhenExplosivePlanted);
        }

        protected override void PrepareProtoWeaponsSkillRanged(SkillConfig config)
        {
            //base.PrepareProtoWeaponsSkillRanged(config);

            // unlock bonus
            config.AddStatEffect(
                StatName.ItemExplosivePlantingTimeMultiplier,
                level: 1,
                percentBonus: -5);

            // every level bonus
            config.AddStatEffect(
                StatName.ItemExplosivePlantingTimeMultiplier,
                formulaPercentBonus: level => -level * 2);

            // lvl 20 bonus
            config.AddStatEffect(
                StatName.ItemExplosivePlantingTimeMultiplier,
                level: 20,
                percentBonus: -5);
        }
    }
}