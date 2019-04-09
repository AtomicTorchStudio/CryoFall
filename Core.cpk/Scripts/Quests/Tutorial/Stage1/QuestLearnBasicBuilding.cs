namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Construction;

    public class QuestLearnBasicBuilding : ProtoQuest
    {
        public override string Description =>
            @"To get access to more structures, you need to unlock ""basic buildings"" in the technology menu.";

        public override string Hints =>
            @"[*] You can quickly access the technologies menu by pressing ""G"" if you haven't changed the controls.
              [*] Unlocking any technology node requires [b]LP[/b] (learning points).
              [*] Learning points are earned from any meaningful activity and are directly proportional to your skills experience gain.";

        public override string Name => "Learn basic building technology";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage1;

        protected override void PrepareQuest(QuestsList prerequisites, RequirementsList requirements)
        {
            requirements
                .Add(RequirementHaveTechNode.Require<TechNodeBasicBuilding>());

            prerequisites
                .Add<QuestBuildACampfire>()
                .Add<QuestMineAnyMineral>();
        }
    }
}