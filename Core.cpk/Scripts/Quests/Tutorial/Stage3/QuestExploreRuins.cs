namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using AtomicTorch.CBND.CoreMod.Tiles;

    public class QuestExploreRuins : ProtoQuest
    {
        public override string Description =>
            "Old ruins are the best place to find rare components and certain resources. It is always a good idea to explore them occasionally, especially if you have the necessary gear.";

        public override string Hints =>
            @"[*] There are different types of ruins: residential (T1), industrial (T2), military (T2) and scientific (T3).
              [*] Difficulty of the ruins ranges from T1, which can be explored by anyone, to T3, which requires the best equipment and meds in CryoFall.
              [*] Exploring ruins is the only way to obtain certain rare items. Chance to find them depends on difficulty of the ruins, and the most valuable items can be found only in T3 ruins.";

        public override string Name => "Explore old ruins";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage3;

        protected override void PrepareQuest(QuestsList prerequisites, RequirementsList requirements)
        {
            requirements
                .Add(RequirementVisitTile.Require<TileRuins>());

            prerequisites
                .Add<QuestCompleteTier1Technologies>();
        }
    }
}