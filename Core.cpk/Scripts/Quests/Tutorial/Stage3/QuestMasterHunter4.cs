namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using AtomicTorch.CBND.CoreMod.Characters.Mobs;

    public class QuestMasterHunter4 : ProtoQuest
    {
        public override string Description =>
            "Time for some extreme hunting! This time not even just animals!";

        public override string Hints =>
            "[*] Be careful—these beasts are extremely aggressive and dangerous. You will need powerful ranged weapons and decent armor to stand a chance.";

        public override string Name => "Master hunter—part four";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage3;

        protected override void PrepareQuest(QuestsList prerequisites, RequirementsList requirements)
        {
            requirements
                .Add(RequirementKill.Require<MobBear>(count: 1))
                .Add(RequirementKill.Require<MobHyena>(count: 1))
                .Add(RequirementKill.Require<MobSnakeBlue>(count: 1))
                .Add(RequirementKill.Require<MobCloakedLizard>(count: 1))
                .Add(RequirementKill.Require<MobBlackBeetle>(count: 1));

            prerequisites
                .Add<QuestMasterHunter3>();
        }
    }
}