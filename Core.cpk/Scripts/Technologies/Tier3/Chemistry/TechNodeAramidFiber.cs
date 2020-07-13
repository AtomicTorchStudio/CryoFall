namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Chemistry
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeAramidFiber : TechNode<TechGroupChemistryT3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeAramidFiber>();

            config.SetRequiredNode<TechNodeSyntheticFiber>();
        }
    }
}