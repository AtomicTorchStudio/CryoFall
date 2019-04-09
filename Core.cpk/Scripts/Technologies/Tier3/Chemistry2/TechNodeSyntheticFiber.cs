namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Chemistry2
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeSyntheticFiber : TechNode<TechGroupChemistry2>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeSyntheticFiber>();

            config.SetRequiredNode<TechNodePlastic>();
        }
    }
}