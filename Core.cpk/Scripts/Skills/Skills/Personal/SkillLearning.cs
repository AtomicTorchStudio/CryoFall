namespace AtomicTorch.CBND.CoreMod.Skills
{
    using AtomicTorch.CBND.CoreMod.Stats;

    public class SkillLearning : ProtoSkill
    {
        /// <summary>
        /// As you earn LP you also get EXP for this skill.
        /// </summary>
        public const double ExperienceAddedPerLPEarned = 10.0;

        public override string Description =>
            "Surviving in the unfamiliar environment of an alien world is difficult, but it offers unique opportunities to learn things you wouldn't know otherwise.";

        public override double ExperienceToLearningPointsConversionMultiplier =>
            0; // this skill doesn't give any LP as it is derivative skill of other skills EXP/LP gain

        public override bool IsSharingLearningPointsWithPartyMembers => false;

        public override string Name => "Learning";

        protected override void PrepareProtoSkill(SkillConfig config)
        {
            config.Category = GetCategory<SkillCategoryPersonal>();

            var statName = StatName.LearningsPointsGainMultiplier;
            config.AddStatEffect(
                statName,
                formulaPercentBonus: level => level); // each level +1% LP gain

            config.AddStatEffect(
                statName,
                level: 1,
                percentBonus: 5);

            config.AddStatEffect(
                statName,
                level: 10,
                percentBonus: 5);

            config.AddStatEffect(
                statName,
                level: 15,
                percentBonus: 5);

            config.AddStatEffect(
                statName,
                level: 20,
                percentBonus: 5);
        }
    }
}