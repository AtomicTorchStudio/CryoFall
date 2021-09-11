namespace AtomicTorch.CBND.CoreMod.Skills
{
    using System.ComponentModel;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;

    public class SkillForaging : ProtoSkill<SkillForaging.Flags>
    {
        public const double ExperienceAddWhenGatheringFinished = 125.0;

        public static readonly DropItemConditionDelegate ConditionAdditionalYield
            = Flags.AdditionalYield.ToCondition();

        public enum Flags
        {
            [Description("Chance to have additional yield")]
            AdditionalYield
        }

        public override string Description =>
            "Understanding of planet's flora increases your harvesting speed and chances to get better yield.";

        public override double ExperienceToLearningPointsConversionMultiplier =>
            0.3; // less exp for foraging, since it is so easy to do

        public override string Name => "Foraging";

        protected override void PrepareProtoSkill(SkillConfig config)
        {
            byte maxLevel = 20;

            config.Category = GetCategory<SkillCategoryAgriculture>();
            config.MaxLevel = maxLevel;

            config.AddStatEffect(
                StatName.ForagingSpeed,
                level: 1,
                percentBonus: 10);

            config.AddStatEffect(
                StatName.ForagingSpeed,
                formulaPercentBonus: level => level * 4);

            config.AddFlagEffect(
                Flags.AdditionalYield,
                level: 10);

            config.AddStatEffect(
                StatName.ForagingSpeed,
                level: maxLevel,
                percentBonus: 10);
        }
    }
}