namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;

    public class QuestGatherResources : ProtoQuest
    {
        public override string Description => "You need to gather some resources to craft basic tools.";

        public override string Hints =>
            @"[*] You can [b]pick up items[/b] from the ground, such as [u]stones[/u], [u]twigs[/u] and [u]grass[/u].
              [*] Simply [b]right click[/b] on an item on the ground and it will be added to your inventory.";

        public override string Name => "Gather resources";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage1;

        protected override void PrepareQuest(QuestsList prerequisites, RequirementsList requirements)
        {
            requirements
                .Add(RequirementHaveItem.Require<ItemFibers>(count: 20))
                .Add(RequirementHaveItem.Require<ItemTwigs>(count: 10))
                .Add(RequirementHaveItem.Require<ItemStone>(count: 10));
        }
    }
}