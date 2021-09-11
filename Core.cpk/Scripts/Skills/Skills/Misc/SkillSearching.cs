namespace AtomicTorch.CBND.CoreMod.Skills
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class SkillSearching : ProtoSkill<SkillSearching.Flags>
    {
        public const double ExperienceAddWhenSearching = 250.0;

        //public static readonly DropItemConditionDelegate ConditionChanceToFindRareItems
        //    = Flags.ChanceToFindRareItems.ToCondition();

        public enum Flags
        {
            // TODO: consider using this flag in the next version
            //[Description("Chance to find rare items")]
            //ChanceToFindRareItems
        }

        public override string Description =>
            "Attention to the tiniest details increases your chances of finding extra items.";

        public override double ExperienceToLearningPointsConversionMultiplier => 1.0;

        public override string Name => "Searching";

        public static bool ServerRollExtraLoot(DropItemContext context)
        {
            if (!context.HasCharacter)
            {
                return false;
            }

            var character = context.Character;
            var extraLootProbability = character.SharedGetFinalStatMultiplier(StatName.SearchingExtraLoot) - 1.0;
            return RandomHelper.RollWithProbability(extraLootProbability);
        }

        protected override void PrepareProtoSkill(SkillConfig config)
        {
            config.Category = GetCategory<SkillCategoryMisc>();

            // searching speed
            config.AddStatEffect(
                StatName.SearchingSpeed,
                level: 1,
                percentBonus: 5);

            config.AddStatEffect(
                StatName.SearchingSpeed,
                formulaPercentBonus: level => 2 * level);

            config.AddStatEffect(
                StatName.SearchingSpeed,
                level: 20,
                percentBonus: 5);

            // extra loot chance
            config.AddStatEffect(
                StatName.SearchingExtraLoot,
                level: 1,
                percentBonus: 2);

            config.AddStatEffect(
                StatName.SearchingExtraLoot,
                formulaPercentBonus: level => level);

            config.AddStatEffect(
                StatName.SearchingExtraLoot,
                level: 15,
                percentBonus: 3);

            //// chance to find rare items
            //config.AddFlagEffect(
            //    Flags.ChanceToFindRareItems,
            //    level: 10);
        }
    }
}