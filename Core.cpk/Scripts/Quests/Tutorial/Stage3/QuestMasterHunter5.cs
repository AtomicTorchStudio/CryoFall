namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using AtomicTorch.CBND.CoreMod.Characters.Mobs;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;

    public class QuestMasterHunter5 : ProtoQuest
    {
        // since there is nothing new to write the text and hints will be the same
        public override string Description => GetProtoEntity<QuestMasterHunter4>().Description;

        public override string Hints => GetProtoEntity<QuestMasterHunter4>().Hints;

        public override string Name => "Master hunter—part five";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage3;

        protected override void PrepareQuest(QuestsList prerequisites, TasksList tasks)
        {
            tasks
                .Add(TaskKill.Require<MobBurrower>(count: 1))
                .Add(TaskKill.Require<MobPragmiumBeetle>(count: 1))
                .Add(TaskKill.Require<MobFireLizard>(count: 1))
                .Add(TaskKill.Require<MobScorpion>(count: 1));

            prerequisites
                .Add<QuestMasterHunter4>()
                .Add<QuestAdvancedResourcesAcquisition>();
        }
    }
}