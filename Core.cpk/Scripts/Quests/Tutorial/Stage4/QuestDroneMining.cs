namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;
    using AtomicTorch.CBND.CoreMod.Items.Drones;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;

    public class QuestDroneMining : ProtoQuest
    {
        public const string HintDroneControl =
            "Use [b]control device[/b] to issue commands to drones.";

        public const string HintDroneTasks =
            "Drones can be used not only to [b]mine minerals[/b] but to [b]fell trees[/b] as well.";

        public const string HintDroneUsage =
            "Drones must be in your [b]inventory[/b] or [b]hotbar[/b] to be issued commands.";

        public override string Description =>
            "Mining minerals and chopping trees by hand is tedious, so you should definitely switch to having automated drones do it for you as soon as you can.";

        public override string Name => "Drone mining";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage4;

        protected override void PrepareQuest(QuestsList prerequisites, TasksList tasks, HintsList hints)
        {
            tasks
                .Add(TaskCraftRecipe.RequireStationRecipe<RecipeDroneControlStandard>())
                .Add(TaskCraftRecipe.RequireStationRecipe<RecipeDroneIndustrialStandard>())
                .Add(TaskUseItem.Require<ItemDroneControlStandard>());

            prerequisites
                .Add<QuestAdvancedResourcesAcquisition>();

            hints
                .Add(HintDroneTasks)
                .Add(HintDroneControl)
                .Add(HintDroneUsage);
        }
    }
}