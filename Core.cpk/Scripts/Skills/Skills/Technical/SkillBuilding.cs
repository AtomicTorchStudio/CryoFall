namespace AtomicTorch.CBND.CoreMod.Skills
{
    using AtomicTorch.CBND.CoreMod.Stats;

    public class SkillBuilding : ProtoSkill
    {
        public const double ExperienceAddWhenBuildOrRepairOneStage = 25.0;

        public const double ExperienceAddWhenDeconstructingOneStage = 5.0;

        public override string Description =>
            "Better building skills improve your construction speed. Applied to all tasks (building, repair) performed with a toolbox.";

        public override double ExperienceToLearningPointsConversionMultiplier =>
            0.25; // this is 1/4 of the standard conversion, because building is very quick and relatively cheap

        public override bool IsSharingLearningPointsWithPartyMembers => true;

        public override string Name => "Building";

        protected override void PrepareProtoSkill(SkillConfig config)
        {
            byte maxLevel = 20;

            config.Category = GetCategory<SkillCategoryTechnical>();
            config.MaxLevel = maxLevel;

            var statName = StatName.BuildingSpeed;
            config.AddStatEffect(
                statName,
                level: 1,
                percentBonus: 10);

            config.AddStatEffect(
                statName,
                formulaPercentBonus: level => level * 4);

            config.AddStatEffect(
                statName,
                level: maxLevel,
                percentBonus: 10);
        }
    }
}