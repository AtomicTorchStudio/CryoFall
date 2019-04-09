namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Chemistry
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;

    public class TechNodeChemicalLab : TechNode<TechGroupChemistry>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectChemicalLab>()
                  .AddRecipe<RecipeFluxPowder>();
        }
    }
}