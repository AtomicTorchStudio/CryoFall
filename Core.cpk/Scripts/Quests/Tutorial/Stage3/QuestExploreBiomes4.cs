namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.CoreMod.Tiles;

    public class QuestExploreBiomes4 : ProtoQuest
    {
        // since there is nothing new to write the text will be the same
        public override string Description => GetProtoEntity<QuestExploreBiomes3>().Description;

        public override string Hints =>
            "[*] [b]Volcanic biome[/b] is extremely dangerous and requires protection from extreme [b]heat[/b] to visit.";

        public override string Name => "Explore biomes—part four";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage3;

        protected override void PrepareQuest(QuestsList prerequisites, TasksList tasks, HintsList hints)
        {
            tasks
                .Add(TaskVisitTile.Require<TileSwamp>())
                .Add(TaskVisitTile.Require<TileVolcanic>());

            prerequisites
                .Add<QuestFastTravel>()
                .Add<QuestFishing>();
        }
    }
}