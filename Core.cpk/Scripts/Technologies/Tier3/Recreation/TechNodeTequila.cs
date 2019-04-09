namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Recreation
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes.FermentationBarrel;

    public class TechNodeTequila : TechNode<TechGroupRecreation>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeTequila>();

            config.SetRequiredNode<TechNodeVodka>();
        }
    }
}