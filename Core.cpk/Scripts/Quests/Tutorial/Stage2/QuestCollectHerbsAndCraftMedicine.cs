namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Items.Medical;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Cooking;

    public class QuestCollectHerbsAndCraftMedicine : ProtoQuest
    {
        public override string Description =>
            "Tough battles will result in you losing your health. You can remedy this by making different medicines that will help you regain your health and cure any afflictions. Just using green herbs won't be enough for much longer.";

        public override string Hints =>
            @"[*] There are many different medicine types in CryoFall. Some restore health, while others can remedy a specific affliction or offer other benefits.
              [*] Crafting advanced medicine types requires learning specific recipes and technologies.
              [*] Herbal remedy can be found in the cooking category.";

        public override string Name => "Gather herbs and make medicine";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage2;

        protected override void PrepareQuest(QuestsList prerequisites, RequirementsList requirements)
        {
            requirements
                .Add(RequirementHaveTechNode.Require<TechNodeHerbalRemedy>())
                .Add(RequirementHaveItem.Require<ItemHerbGreen>(count: 2, isReversible: false))
                .Add(RequirementHaveItem.Require<ItemMushroomRust>(count: 1, isReversible: false))
                .Add(RequirementHaveItem.Require<ItemWaterbulb>(count: 1, isReversible: false))
                .Add(RequirementCraftRecipe.RequireStationRecipe<RecipeHerbalRemedy>());

            prerequisites
                .Add<QuestFindAndUseGreenHerb>();
        }
    }
}