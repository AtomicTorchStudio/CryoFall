namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.CoreMod.Tiles;

    public class QuestExploreBiomes3 : ProtoQuest
    {
        public override string Description =>
            "There are also some harder to reach biomes that could present additional challenges, but offer some unique resources.";

        public override string Hints =>
            @"[*] Barren biome is the home of many desert animals.
              [*] Salt flats can be a great source of large quantities of salt.";

        public override string Name => "Explore biomes—part three";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage3;

        protected override void PrepareQuest(QuestsList prerequisites, TasksList tasks)
        {
            tasks
                .Add(TaskVisitTile.Require<TileSaltFlats>())
                .Add(TaskVisitTile.Require<TileBarren>());

            prerequisites
                .Add<QuestEstablishPowerGrid>()
                .Add<QuestBuildMedicalLab>();
        }
    }
}