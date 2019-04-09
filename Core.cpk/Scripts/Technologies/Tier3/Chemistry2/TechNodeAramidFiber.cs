namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Chemistry2
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeAramidFiber : TechNode<TechGroupChemistry2>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeAramidFiber>();

            config.SetRequiredNode<TechNodeSyntheticFiber>();
        }
    }
}