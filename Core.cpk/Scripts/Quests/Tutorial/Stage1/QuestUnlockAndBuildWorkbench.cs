namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Industry;

    public class QuestUnlockAndBuildWorkbench : ProtoQuest
    {
        public override string Description =>
            "The workbench serves as your main crafting station for the majority of recipes in CryoFall. Building one should be a priority when establishing a new base.";

        public override string Hints =>
            "[*] In the technology menu, you can see where each recipe is crafted. This information is displayed under the recipe's name.";

        public override string Name => "Unlock and build a workbench";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage1;

        protected override void PrepareQuest(QuestsList prerequisites, TasksList tasks)
        {
            tasks
                .Add(TaskHaveTechNode.Require<TechNodeWorkbench>())
                .Add(TaskBuildStructure.Require<ObjectWorkbench>());

            prerequisites
                .Add<QuestBuildABedroll>();
        }
    }
}