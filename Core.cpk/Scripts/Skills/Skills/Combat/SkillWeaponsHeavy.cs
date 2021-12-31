namespace AtomicTorch.CBND.CoreMod.Skills
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class SkillWeaponsHeavy : ProtoSkillWeaponsRanged
    {
        private const double ExperienceAddWhenExplosivePlanted = 250.0;

        public override string Description =>
            "Heavy weaponry usage trains you to operate it with greater skill, providing additional means to bring on the hurt.";

        public override double ExperienceAddedOnKillPerMaxEnemyHealthMultiplier => 0.2;

        public override double ExperienceAddedPerDamageDoneMultiplier => 0.5;

        /// <summary>
        /// This is intended to reward experience per ammo expended. Basically resource->exp conversion.
        /// </summary>
        public override double ExperienceAddedPerShot => 25;

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
            // faster explosion planting
            // unlock bonus
            config.AddStatEffect(
                StatName.ItemExplosivePlantingSpeedMultiplier,
                level: 1,
                percentBonus: 20);

            // every level bonus
            config.AddStatEffect(
                StatName.ItemExplosivePlantingSpeedMultiplier,
                formulaPercentBonus: level => level * 3);

            // lvl 20 bonus
            config.AddStatEffect(
                StatName.ItemExplosivePlantingSpeedMultiplier,
                level: 20,
                percentBonus: 20);

            // other bonuses
            config.AddStatEffect(this.StatNameReloadingSpeedMultiplier.Value,
                                 formulaPercentBonus: level => -level * 2);

            config.AddStatEffect(this.StatNameDegrationRateMultiplier,
                                 formulaPercentBonus: level => -level * 2);
        }
    }
}