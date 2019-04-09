namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using AtomicTorch.CBND.CoreMod.Tiles;

    public class QuestExploreBiomes3 : ProtoQuest
    {
        public override string Description =>
            "There are also some harder to reach biomes that could present additional challenges, but offer some unique resources.";

        public override string Hints =>
            @"[*] Barren biome is the home of many desert animals.
              [*] Salt flats can be a great source of large quantities of salt.";

        public override string Name => "Explore biomes—part three";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage2;

        protected override void PrepareQuest(QuestsList prerequisites, RequirementsList requirements)
        {
            requirements
                .Add(RequirementVisitTile.Require<TileSaltFlats>())
                .Add(RequirementVisitTile.Require<TileBarren>());

            prerequisites
                .Add<QuestExploreRuins>()
                .Add<QuestBuildMedicalLab>();
        }
    }
}