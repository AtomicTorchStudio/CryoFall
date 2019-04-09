namespace AtomicTorch.CBND.CoreMod.Skills
{
    using System.ComponentModel;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;

    public class SkillFarming : ProtoSkill<SkillFarming.Flags>
    {
        public const double ExperienceForFertilizing = 100.0; // 0.25 LP

        public const double ExperienceForHarvesting = 25.0; // 0.0625 LP

        public const double ExperienceForSeedPlanting = 50.0; // 0.125 LP

        public const double ExperienceForWatering = 25.0; // 0.0625 LP

        public static readonly DropItemConditionDelegate ConditionExtraYield
            // requires flag
            = context => context.Character.SharedHasSkillFlag(Flags.AdditionalYield);

        public enum Flags
        {
            [Description("Chance to have additional yield")]
            AdditionalYield
        }

        public override string Description =>
            "Better farming techniques allow you to improve your farming efficiency, plant growth rate, ripening speed and even have extra yield occasionally.";

        public override double ExperienceToLearningPointsConversionMultiplier =>
            0.25; // this is 1/4 of the standard conversion, because it is possible to make a huge farm quickly

        public override bool IsSharingLearningPointsWithPartyMembers => true;

        public override string Name => "Farming";

        protected override void PrepareProtoSkill(SkillConfig config)
        {
            byte maxLevel = 20;

            config.Category = GetCategory<SkillCategoryAgriculture>();
            config.MaxLevel = maxLevel;

            // Level 1: speed of farming tasks
            config.AddStatEffect(
                StatName.FarmingTasksSpeed,
                level: 1,
                percentBonus: 10);

            // Level 1: plants grow speed
            config.AddStatEffect(
                StatName.FarmingPlantGrowSpeed,
                level: 1,
                percentBonus: 10);

            // Speed of farming tasks (such as plant gathering or watering, etc.)
            config.AddStatEffect(
                StatName.FarmingTasksSpeed,
                formulaPercentBonus: level => level * 4);

            // Plants grow speed
            config.AddStatEffect(
                StatName.FarmingPlantGrowSpeed,
                formulaPercentBonus: level => level * 4);

            // Level 10: Chance for extra yield (the effect is dependent on a particular plant, so no specific number here, it is just a flag)
            config.AddFlagEffect(
                Flags.AdditionalYield,
                level: 10);

            // Level 20: speed of farming tasks
            config.AddStatEffect(
                StatName.FarmingTasksSpeed,
                level: maxLevel,
                percentBonus: 10);

            // Level 20: plants grow speed
            config.AddStatEffect(
                StatName.FarmingPlantGrowSpeed,
                level: maxLevel,
                percentBonus: 10);
        }
    }
}