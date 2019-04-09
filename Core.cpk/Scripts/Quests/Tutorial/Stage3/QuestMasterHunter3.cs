namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using AtomicTorch.CBND.CoreMod.Characters.Mobs;

    public class QuestMasterHunter3 : ProtoQuest
    {
        public override string Description =>
            "Time for even more hunting! This time with more dangerous animals.";

        public override string Hints =>
            "[*] Be careful—these animals are aggressive, and it's best to use decent long-range weapons when hunting them.";

        public override string Name => "Master hunter—part three";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage3;

        protected override void PrepareQuest(QuestsList prerequisites, RequirementsList requirements)
        {
            requirements
                .Add(RequirementKill.Require<MobWolf>(count: 1))
                .Add(RequirementKill.Require<MobSnakeBrown>(count: 1))
                .Add(RequirementKill.Require<MobCrawler>(count: 1))
                .Add(RequirementKill.Require<MobWildBoar>(count: 1))
                .Add(RequirementKill.Require<MobHoneyBadger>(count: 1));

            prerequisites
                .Add<QuestExploreRuins>()
                .Add<QuestBuildChemicalLab>();
        }
    }
}