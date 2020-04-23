namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Minerals;

    public class QuestAcquirePragmium : ProtoQuest
    {
        public override string Description =>
            "Pragmium is a rare alien mineral needed for the most advanced technologies. Try to find and acquire some.";

        public override string Hints =>
            @"[*] Beware! Mining pragmium is very dangerous.
              [*] Large pragmium spires can be commonly found in the desert, while small nodes can often be found near volcanoes.";

        public override string Name => "Acquire pragmium!";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage3;

        protected override void PrepareQuest(QuestsList prerequisites, TasksList tasks)
        {
            tasks
                .Add(TaskDestroy.Require<ObjectMineralPragmiumSource>(count: 1))
                .Add(TaskDestroy.Require<ObjectMineralPragmiumNode>(count: 5))
                .Add(TaskHaveItem.Require<ItemOrePragmium>(count: 10, isReversible: false));

            prerequisites
                .Add<QuestMasterHunter4>();
        }
    }
}