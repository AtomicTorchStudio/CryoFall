namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using AtomicTorch.CBND.CoreMod.Items.Seeds;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Farms;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Farming;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class QuestBuildAFarm : ProtoQuest
    {
        public const string TaskPlantAnySeeds = "Plant any seeds";

        public override string Description =>
            "Farming is a good and sustainable way to obtain vast quantities of food. Time to get your hands dirty!";

        public override string Hints =>
            @"[*] [b]Seeds[/b] can be found randomly while simply [b]collecting grass[/b]. Some special seeds can only be found in the ruins.
              [*] It is a good idea to build your farm [b]inside[/b] the protection of your walls, as other survivors may decide to pick your crops for you :)
              [*] Different plants vary in how long they take to reach maturity, the number of harvests they yield, and other properties.
              [*] Later you can research better fertilizers and other things to improve the efficiency of your farm.";

        public override string Name => "Build a farm";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage2;

        protected override void PrepareQuest(QuestsList prerequisites, TasksList tasks, HintsList hints)
        {
            var listSeeds = Api.FindProtoEntities<IProtoItemSeed>();
            listSeeds.RemoveAll(i => i is IProtoItemSapling);

            tasks
                .Add(TaskHaveTechNode.Require<TechNodeFarmingBasics>())
                .Add(TaskBuildStructure.Require<ObjectFarmPlot>())
                .Add(TaskBuildStructure.Require<ObjectFarmingWorkbench>())
                .Add(TaskUseItem.Require(listSeeds, description: TaskPlantAnySeeds)
                                .WithIcon(listSeeds[0].Icon));

            prerequisites
                .Add<QuestCollectHerbsAndCraftMedicine>();
        }
    }
}