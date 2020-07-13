namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Characters.Mobs;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;

    public class QuestMasterHunter4 : ProtoQuest
    {
        public override string Description =>
            "Time for some extreme hunting! This time not even just animals!";

        public override string Hints =>
            "[*] Be careful—these beasts are extremely aggressive and dangerous. You will need powerful ranged weapons and decent armor to stand a chance.";

        public override string Name => "Master hunter—part four";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage3;

        protected override void PrepareQuest(QuestsList prerequisites, TasksList tasks, HintsList hints)
        {
            tasks
                .Add(TaskKill.Require<MobBear>(count: 1))
                .Add(TaskKill.Require<MobHyena>(count: 1))
                .Add(TaskKill.Require<MobSnakeBlue>(count: 1))
                .Add(TaskKill.Require<MobCloakedLizard>(count: 1))
                .Add(TaskKill.Require<MobBlackBeetle>(count: 1));

            prerequisites
                .Add<QuestMasterHunter3>();
        }
    }
}