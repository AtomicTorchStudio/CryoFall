namespace AtomicTorch.CBND.CoreMod.Skills
{
    using AtomicTorch.CBND.CoreMod.Stats;

    public class SkillWoodcutting : ProtoSkill
    {
        public const double ExperienceAddPerStructurePoint = 0.25; // (600 HP -> 150 XP)

        public override string Description =>
            "Better training and understanding of this profession improve your woodcutting speed. Applies only when you're using an axe.";

        public override double ExperienceToLearningPointsConversionMultiplier =>
            0.5; // slightly lower since each tree gives 150 exp (which would mean 1.5LP without this multiplier)

        public override bool IsSharingLearningPointsWithPartyMembers => true;

        public override string Name => "Woodcutting";

        protected override void PrepareProtoSkill(SkillConfig config)
        {
            config.Category = GetCategory<SkillCategoryProduction>();

            // level bonus
            config.AddStatEffect(
                StatName.WoodcuttingSpeed,
                formulaPercentBonus: level => level * 2);

            // lvl 20 bonus
            config.AddStatEffect(
                StatName.WoodcuttingSpeed,
                level: 20,
                percentBonus: 10);
        }
    }
}