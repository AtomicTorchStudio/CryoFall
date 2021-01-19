namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Misc;

    public class QuestFastTravel : ProtoQuest
    {
        public const string HintFindMany =
            "The [b]more[/b] teleport locations you find the [b]wider[/b] your travel network will be.";

        public const string HintFindTwo =
            "Find at least [b]two[/b] teleport locations to be able to travel between them.";

        public const string HintHiddenLocations =
            "There might also be [b]hidden[/b] teleport locations!";

        public override string Description =>
            "Running around on your legs is fine, but to travel really long distances you might want to use alien teleports.";

        public override string Name => "Fast travel";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage3;

        protected override void PrepareQuest(QuestsList prerequisites, TasksList tasks, HintsList hints)
        {
            tasks
                .Add(TaskDiscoverTeleport.Require<ObjectAlienTeleport>(3));

            prerequisites
                .Add<QuestExploreBiomes3>();

            hints
                .Add(HintFindTwo)
                .Add(HintFindMany)
                .Add(HintHiddenLocations);
        }
    }
}