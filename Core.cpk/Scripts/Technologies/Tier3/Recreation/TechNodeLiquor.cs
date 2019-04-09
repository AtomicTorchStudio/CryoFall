namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Recreation
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes.FermentationBarrel;

    public class TechNodeLiquor : TechNode<TechGroupRecreation>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeLiquor>();

            config.SetRequiredNode<TechNodeFermentationBarrel>();
        }
    }
}