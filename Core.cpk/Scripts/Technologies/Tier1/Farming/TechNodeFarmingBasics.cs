namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.Farming
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Farms;

    public class TechNodeFarmingBasics : TechNode<TechGroupFarming>
    {
        public override string Name => "Farming basics";

        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectFarmPlot>()
                  .AddStructure<ObjectFarmingWorkbench>()
                  .AddRecipe<RecipeSeedsCarrot>();
        }
    }
}