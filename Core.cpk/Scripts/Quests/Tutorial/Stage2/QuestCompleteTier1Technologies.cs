namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Construction;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Cooking;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Farming;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Industry;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.OffenseAndDefense;

    public class QuestCompleteTier1Technologies : ProtoQuest
    {
        public override string Description =>
            "Higher level technologies require mastery of early tech to progress further.";

        public override string Hints =>
            "[*] You can unlock higher tiers of technological groups to gain access to more complex technologies, recipes and structures.";

        public override string Name => "Mastering technologies—part one";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage2;

        protected override void PrepareQuest(QuestsList prerequisites, RequirementsList requirements)
        {
            requirements
                .Add(RequirementCompleteTechGroup.Require<TechGroupConstruction>())
                .Add(RequirementCompleteTechGroup.Require<TechGroupIndustry>())
                .Add(RequirementCompleteTechGroup.Require<TechGroupOffenseAndDefense>())
                .Add(RequirementCompleteTechGroup.Require<TechGroupFarming>())
                .Add(RequirementCompleteTechGroup.Require<TechGroupCooking>());

            prerequisites
                .Add<QuestExploreBiomes2>()
                .Add<QuestMasterHunter2>()
                .Add<QuestBuildALampCraftCampfuel>();
        }
    }
}