namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Loot;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class QuestGatherResources : ProtoQuest
    {
        public override string Description => "You need to gather some resources to craft basic tools.";

        public override string Hints =>
            @"[*] You can [b]pick up items[/b] from the ground, such as [u]stones[/u], [u]twigs[/u] and [u]grass[/u].
              [*] Simply [b]right click[/b] on an item on the ground and it will be added to your inventory.";

        public override string Name => "Gather resources";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage1;

        protected override void PrepareQuest(QuestsList prerequisites, TasksList tasks, HintsList hints)
        {
            ITextureResource iconGrass = null,
                             iconTwigs = null,
                             iconStone = null;

            if (IsClient)
            {
                iconGrass = Api.GetProtoEntity<ObjectLootGrass>().Icon;
                iconTwigs = Api.GetProtoEntity<ObjectLootTwigs>().Icon;
                iconStone = Api.GetProtoEntity<ObjectLootStone>().Icon;
            }

            tasks
                .Add(TaskHaveItem.Require<ItemFibers>(count: 10).WithIcon(iconGrass))
                .Add(TaskHaveItem.Require<ItemTwigs>(count: 10).WithIcon(iconTwigs))
                .Add(TaskHaveItem.Require<ItemStone>(count: 10).WithIcon(iconStone));
        }
    }
}