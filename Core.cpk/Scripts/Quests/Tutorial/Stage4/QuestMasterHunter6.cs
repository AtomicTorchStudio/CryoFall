namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using AtomicTorch.CBND.CoreMod.Characters.Mobs;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;

    public class QuestMasterHunter6 : ProtoQuest
    {
        // since there is nothing new to write the text and hints will be the same
        public override string Description => GetProtoEntity<QuestMasterHunter5>().Description;

        public override string Hints => GetProtoEntity<QuestMasterHunter5>().Hints;

        public override string Name => "Master hunter—part six";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage4;

        protected override void PrepareQuest(QuestsList prerequisites, TasksList tasks, HintsList hints)
        {
            tasks
                .Add(TaskKill.Require<MobThumper>(count: 1))
                .Add(TaskKill.Require<MobSpitter>(count: 1))
                .Add(TaskKill.Require<MobPsiGrove>(count: 1))
                .Add(TaskKill.Require<MobFloater>(count: 1));

            prerequisites
                .Add<QuestMasterHunter5>()
                .Add<QuestDroneMining>();
        }
    }
}