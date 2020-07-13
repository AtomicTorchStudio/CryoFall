namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Minerals;
    using AtomicTorch.CBND.CoreMod.Tiles;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class QuestMineAnyMineral : ProtoQuest
    {
        public const string TaskMineStone = "Mine stone";

        public override string Description =>
            "Let's put this pickaxe to use. Mine some minerals and see what you get. You will also need to mine some stone from large rocks.";

        public override string Hints =>
            @"[*] As with any other usable tool, you need to place the pickaxe into your [b]hotbar[/b].
              [*] The best place to find mineral nodes is in the [b]mountains[/b]. You can use your map (""M"" by default) to locate a good spot.
              [*] Different minerals take different times to mine and give different types of resources.
              [*] The best way to obtain [b]vast quantities[/b] of stone is to [b]mine it[/b] from large rocks.
              [*] Certain minerals may also give rare drops such as gems or gold nuggets if your mining skill is high enough.";

        public override string Name => "Mine any mineral";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage1;

        protected override void PrepareQuest(QuestsList prerequisites, TasksList tasks, HintsList hints)
        {
            // minerals without stone (as it has a separate requirement)
            var listMinerals = Api.FindProtoEntities<IProtoObjectMineral>();
            listMinerals.Remove(Api.GetProtoEntity<ObjectMineralStone>());

            tasks
                .Add(TaskVisitTile.Require<TileRocky>())
                .Add(TaskDestroy.Require(list: listMinerals, count: 3, description: this.Name)
                                .WithIcon(Api.IsClient
                                              ? ClientItemIconHelper.CreateComposedIcon(this.ShortId,
                                                                                        Api.GetProtoEntity<ObjectMineralIron>().Icon,
                                                                                        Api.GetProtoEntity<ObjectMineralCopper>().Icon)
                                              : null))
                .Add(TaskDestroy.Require<ObjectMineralStone>(count: 3, description: TaskMineStone));

            prerequisites
                .Add<QuestCraftAPickaxe>();
        }
    }
}