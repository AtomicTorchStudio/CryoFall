namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Industry;

    public class QuestBuildFurnaceAndSmeltCopper : ProtoQuest
    {
        public override string Description =>
            "You will need to unlock and build a furnace to be able to smelt copper and other metals. Metals serve as the primary resource and foundation for all industrial processes in CryoFall.";

        public override string Hints =>
            @"[*] In the technology menu, you can see where each recipe is crafted. This information is displayed under the recipe's name.
              [*] You will need to mine some copper ore nodes to get the necessary ore for smelting.";

        public override string Name => "Build a furnace and smelt copper";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage2;

        protected override void PrepareQuest(QuestsList prerequisites, TasksList tasks, HintsList hints)
        {
            tasks
                .Add(TaskHaveTechNode.Require<TechNodeSmelting>())
                .Add(TaskBuildStructure.Require<ObjectFurnace>())
                .Add(TaskManufactureItem.Require<ItemIngotCopper>(count: 10));

            prerequisites
                .Add<QuestCollectHerbsAndCraftMedicine>();
        }
    }
}