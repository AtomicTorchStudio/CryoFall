namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using AtomicTorch.CBND.CoreMod.Items.Seeds;
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
            @"[*] It is a good idea to build your farm [b]inside[/b] the protection of your walls, as other survivors may decide to pick your crops for you :)
              [*] Different plants vary in how long they take to reach maturity, their number of harvests, and other properties.
              [*] Later you can research better fertilizers and other things to improve the efficiency of your farm.";

        public override string Name => "Build a farm";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage2;

        protected override void PrepareQuest(QuestsList prerequisites, RequirementsList requirements)
        {
            var listSeeds = Api.FindProtoEntities<IProtoItemSeed>();
            listSeeds.RemoveAll(i => i is IProtoItemSapling);

            requirements
                .Add(RequirementHaveTechNode.Require<TechNodeFarmingBasics>())
                .Add(RequirementBuildStructure.Require<ObjectFarmPlot>())
                .Add(RequirementBuildStructure.Require<ObjectFarmingWorkbench>())
                .Add(RequirementUseItem.Require(listSeeds, description: TaskPlantAnySeeds));

            prerequisites
                .Add<QuestBuildAPermanentBase>();
        }
    }
}