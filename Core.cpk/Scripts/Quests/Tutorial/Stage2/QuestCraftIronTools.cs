namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.CraftRecipes;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Industry;

    public class QuestCraftIronTools : ProtoQuest
    {
        public override string Description =>
            "Now that you have access to metallurgy, it is only natural to upgrade your tools. Iron tools are not only more durable, but also much more effective than primitive stone tools.";

        public override string Hints =>
            "[*] Try to always upgrade to better tools as they become available, since it gives you a significant productivity boost.";

        public override string Name => "Craft better tools";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage2;

        protected override void PrepareQuest(QuestsList prerequisites, TasksList tasks, HintsList hints)
        {
            tasks
                .Add(TaskHaveTechNode.Require<TechNodeIronTools>())
                .Add(TaskCraftRecipe.RequireStationRecipe<RecipeAxeIron>())
                .Add(TaskCraftRecipe.RequireStationRecipe<RecipePickaxeIron>());

            prerequisites
                .Add<QuestBuildFurnaceAndSmeltCopper>();
        }
    }
}