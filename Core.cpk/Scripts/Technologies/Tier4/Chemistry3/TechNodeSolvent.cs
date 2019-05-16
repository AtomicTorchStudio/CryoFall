namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Chemistry3
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeSolvent : TechNode<TechGroupChemistry3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeSolvent>();
        }
    }
}