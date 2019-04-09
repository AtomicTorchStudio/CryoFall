namespace AtomicTorch.CBND.CoreMod.Skills
{
    using AtomicTorch.CBND.CoreMod.Stats;

    public class SkillLearning : ProtoSkill
    {
        /// <summary>
        /// As you earn LP you also get EXP for this skill.
        /// </summary>
        public const double ExperienceAddedPerLPEarned = 10.0;

        /// <summary>
        /// When you die the game checks how much LP you lost and converts it to EXP for this skill.
        /// </summary>
        public const double ExperienceAddedPerLPLost = 50.0;

        public const ushort LearningPointsRetainedAfterDeathBaseValue = 20;

        public override string Description =>
            "Surviving in the unfamiliar environment of an alien world is difficult, but it offers unique opportunities to learn things you wouldn't know otherwise.";

        public override double ExperienceToLearningPointsConversionMultiplier =>
            0; // this skill doesn't give any LP as it is derivative skill of other skills EXP/LP gain

        public override bool IsSharingLearningPointsWithPartyMembers => false;

        public override string Name => "Learning";

        protected override void PrepareProtoSkill(SkillConfig config)
        {
            config.Category = GetCategory<SkillCategoryPersonal>();

            // Skill effects:
            // each level +5 LP retained after death
            var statName = StatName.LearningPointsRetainedAfterDeath;
            config.AddStatEffect(
                statName,
                formulaValueBonus: level => level * 5);

            // level 10:
            config.AddStatEffect(
                statName,
                level: 10,
                valueBonus: 10);

            // level 15:
            config.AddStatEffect(
                statName,
                level: 15,
                valueBonus: 20);

            // level 20:
            config.AddStatEffect(
                statName,
                level: 20,
                valueBonus: 50);

            // Please note: default value is 20 LP retained after death + bonus from this skill.
            // At maximum level 200LP is retained after death.
        }
    }
}