namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using AtomicTorch.CBND.CoreMod.Characters.Mobs;

    public class QuestMasterHunter5 : ProtoQuest
    {
        // since there is nothing new to write the text and hints will be the same
        public override string Description => GetProtoEntity<QuestMasterHunter4>().Description;
        public override string Hints => GetProtoEntity<QuestMasterHunter4>().Hints;

        public override string Name => "Master hunter—part five";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage3;

        protected override void PrepareQuest(QuestsList prerequisites, RequirementsList requirements)
        {
            requirements
                .Add(RequirementKill.Require<MobBurrower>(count: 1))
                .Add(RequirementKill.Require<MobPragmiumBeetle>(count: 1))
                .Add(RequirementKill.Require<MobFireLizard>(count: 1))
                .Add(RequirementKill.Require<MobScorpion>(count: 1));

            prerequisites
                .Add<QuestMasterHunter4>()
                .Add<QuestAdvancedResourcesAcquisition>();
        }
    }
}