namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Minerals;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Bushes;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Industry2;

    public class QuestAdvancedResourcesAcquisition : ProtoQuest
    {
        public const string TaskMineSalt = "Mine salt mineral";

        public override string Description =>
            "Certain resources such as petroleum oil and lithium are necessary for advanced technology. It is critically important to learn how to acquire them.";

        public override string Hints =>
            @"[*] You can find oilpod fruits in the [u]boreal forest[/u] growing everywhere. Using them allows you to extract small quantities of petroleum oil.
              [*] You can get [u]lithium salts[/u] when mining regular salt minerals.
              [*] You can find salt minerals in the [u]salt flats[/u] in the desert.
              [*] Learning [u]Xenogeology[/u] offers industrial-scale methods of acquiring these resources.";

        public override string Name => "Advanced resources acquisition";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage3;

        protected override void PrepareQuest(QuestsList prerequisites, RequirementsList requirements)
        {
            requirements
                .Add(RequirementDestroy.Require<ObjectMineralSalt>(count: 5, description: TaskMineSalt))
                .Add(RequirementGather.Require<ObjectBushOilpod>(count: 5))
                .Add(RequirementHaveTechNode.Require<TechNodePetroleumFromOilpods>())
                .Add(RequirementCraftRecipe.RequireStationRecipe<RecipeCanisterPetroleum>());

            prerequisites
                .Add<QuestExploreBiomes3>();
        }
    }
}