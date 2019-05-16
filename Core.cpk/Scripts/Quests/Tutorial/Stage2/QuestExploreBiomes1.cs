namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using AtomicTorch.CBND.CoreMod.Tiles;

    public class QuestExploreBiomes1 : ProtoQuest
    {
        public override string Description =>
            "Each different biome has unique flora and fauna associated with it, in addition to other properties. It is a good idea to familiarize yourself with each of the basic biomes to extract maximum use out of them.";

        public override string Hints =>
            @"[*] Meadows have large numbers of bushes, herbs and other useful plants to forage.
              [*] You can find many aquatic animals near a shore.";

        public override string Name => "Explore biomes—part one";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage2;

        protected override void PrepareQuest(QuestsList prerequisites, RequirementsList requirements)
        {
            requirements
                .Add(RequirementVisitTile.Require<TileForestTemperate>())
                .Add(RequirementVisitTile.Require<TileForestTropical>())
                .Add(RequirementVisitTile.Require<TileBeachTemperate>())
                .Add(RequirementVisitTile.Require<TileLakeShore>())
                .Add(RequirementVisitTile.Require<TileMeadows>());

            prerequisites
                .Add<QuestClaySandGlassBottlesWaterCollector>();
        }
    }
}