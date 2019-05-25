namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Chemistry;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Construction2;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Cooking2;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Defense2;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Farming2;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Industry2;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Medicine;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Offense2;

    public class QuestCompleteTier2Technologies : ProtoQuest
    {
        public override string Description =>
            "At this point it might be a good idea to complete all of the Tier 2 technologies, to have a solid foundation for further specialization in a particular area.";

        public override string Hints =>
            @"[*] Early technologies (Tier 1 and 2) are relatively cheap to unlock, so it is always a good idea to learn all of them.
              [*] For advanced technologies (Tier 3 and up), it is a good idea to specialize in just a few. Consider which of them are most important to you, rather then trying to learn all of them.";

        public override string Name => "Mastering technologies—part two";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage3;

        protected override void PrepareQuest(QuestsList prerequisites, RequirementsList requirements)
        {
            requirements
                .Add(RequirementCompleteTechGroup.Require<TechGroupChemistry>())
                .Add(RequirementCompleteTechGroup.Require<TechGroupConstruction2>())
                .Add(RequirementCompleteTechGroup.Require<TechGroupCooking2>())
                .Add(RequirementCompleteTechGroup.Require<TechGroupDefense2>())
                .Add(RequirementCompleteTechGroup.Require<TechGroupFarming2>())
                .Add(RequirementCompleteTechGroup.Require<TechGroupIndustry2>())
                .Add(RequirementCompleteTechGroup.Require<TechGroupMedicine>())
                .Add(RequirementCompleteTechGroup.Require<TechGroupOffense2>());

            prerequisites
                .Add<QuestMasterHunter4>()
                .Add<QuestExploreBiomes3>();
        }
    }
}