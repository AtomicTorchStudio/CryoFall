namespace AtomicTorch.CBND.CoreMod.Skills
{
    using System.ComponentModel;
    using AtomicTorch.CBND.CoreMod.Stats;

    public class SkillFishing : ProtoSkill<SkillFishing.Flags>
    {
        public const double BaseFishCatchChancePercents = 70;

        public const double ExperienceForCaughtFish = 1000.0;

        public const double FishingChanceToSaveBait = 0.15; // 15%

        public const string SkillLevelRequirement = "Requirement: Fishing skill level {0}.";

        public enum Flags
        {
            [Description("Chance to save the bait")]
            FishingChanceToSaveBait
        }

        public override string Description =>
            "Your knowledge about different fish species and their habits represents your ability to succeed while fishing.";

        public override double ExperienceToLearningPointsConversionMultiplier => 0.2; // effectively 2 LP per fish

        public override bool IsSharingLearningPointsWithPartyMembers => true;

        public override string Name => "Fishing";

        protected override void PrepareProtoSkill(SkillConfig config)
        {
            byte maxLevel = 20;

            config.Category = GetCategory<SkillCategoryAgriculture>();
            config.MaxLevel = maxLevel;

            config.AddStatEffect(
                StatName.FishingKnowledgeLevel,
                formulaValueBonus: level => 5 * level,
                displayTotalValue: true);

            // improve on +1% with every level (base value is 80% see BaseFishCatchChancePercents)
            config.AddStatEffect(
                StatName.FishingSuccess,
                formulaPercentBonus: level => level,
                displayTotalValue: true);

            config.AddFlagEffect(
                Flags.FishingChanceToSaveBait,
                level: 10);
        }
    }
}