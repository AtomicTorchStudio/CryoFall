namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Generators;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.PowerStorage;

    public class QuestPowerGrid2 : ProtoQuest
    {
        public const string HintGridExpansion =
            "It is a good idea to [u]expand your power storage capacity[/u] as your base grows to make sure you always have [u]sufficient surplus[/u].";

        public override string Description =>
            "Now that you have a solid foundation for your power grid, it might be a good idea to expand it with more useful structures to make your life more convenient.";

        public override string Name => "More use for power grid";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage4;

        protected override void PrepareQuest(QuestsList prerequisites, TasksList tasks, HintsList hints)
        {
            tasks
                .Add(TaskBuildStructure.Require<ObjectGeneratorBio>())
                .Add(TaskBuildStructure.Require<ObjectPowerStorageLarge>())
                .Add(TaskBuildStructure.Require<ObjectFurnaceElectric>())
                .Add(TaskBuildStructure.Require<ObjectRechargingStation>());

            prerequisites
                .Add<QuestCompleteTier2Technologies>();

            hints
                .Add(HintGridExpansion);
        }
    }
}