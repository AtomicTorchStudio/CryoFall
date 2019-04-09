namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Construction3
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeBricks : TechNode<TechGroupConstruction3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeBricks>();

            config.SetRequiredNode<TechNodeLandClaimT3>();
        }
    }
}