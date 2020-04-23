namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Fridges;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Construction;

    public class QuestBuildEvaporativeFridge : ProtoQuest
    {
        public override string Description =>
            "What's the good in collecting all that food if you cannot store it for later? Thankfully there is a solution for that, too.";

        public override string Hints =>
            @"[*] Fridges extend the time until food spoils. The better the fridge—the greater this effect.
              [*] The time it takes for food to spoil also depends on the particular type of food. Meat spoils very quickly, while canned food stores for ages.";

        public override string Name => "Build a simple fridge";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage2;

        protected override void PrepareQuest(QuestsList prerequisites, TasksList tasks)
        {
            tasks
                .Add(TaskHaveTechNode.Require<TechNodeFridgeEvaporator>())
                .Add(TaskBuildStructure.Require<ObjectFridgeEvaporator>());

            prerequisites
                .Add<QuestBuildMulchboxAndCraftWateringCan>();
        }
    }
}