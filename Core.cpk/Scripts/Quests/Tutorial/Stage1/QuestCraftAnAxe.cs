namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class QuestCraftAnAxe : ProtoQuest
    {
        public override string Description =>
            "Now that you have some resources, you can start crafting. First, craft a rope. Then, when it's done—craft an axe.";

        public override string Hints =>
            @"[*] If you haven't changed the default keyboard bindings, you can quickly open the crafting menu with ""C"" and inventory with ""E"".";

        public override string Name => "Craft an axe";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage1;

        protected override void PrepareQuest(QuestsList prerequisites, RequirementsList requirements)
        {
            requirements
                .Add(RequirementCraftRecipe.RequireHandRecipe<RecipeRope>())
                .Add(RequirementCraftRecipe.RequireHandRecipe<RecipeAxeStone>());

            prerequisites
                .Add<QuestGatherResources>();
        }
    }
}