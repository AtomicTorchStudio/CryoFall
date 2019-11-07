namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using AtomicTorch.CBND.CoreMod.Tiles;

    public class QuestExploreBiomes2 : ProtoQuest
    {
        public override string Description =>
            "Different biomes have different resources that can be found there and animals that inhabit the area. It is a good idea to explore as much around your base as possible to see what's available.";

        public override string Hints =>
            @"[*] Rocky mountains biome is a great way to find many mineral deposits.
              [*] Clay pits are the primary source of vast quantities of clay for construction.
              [*] Boreal forest can be found in the north of the continent.";

        public override string Name => "Explore biomes—part two";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage2;

        protected override void PrepareQuest(QuestsList prerequisites, RequirementsList requirements)
        {
            requirements
                .Add(RequirementVisitTile.Require<TileForestBoreal>())
                .Add(RequirementVisitTile.Require<TileRocky>())
                .Add(RequirementVisitTile.Require<TileClay>())
                .Add(RequirementVisitTile.Require<TileRoads>());

            prerequisites
                .Add<QuestUseCrowbarAndDeconstructBuilding>();
        }
    }
}