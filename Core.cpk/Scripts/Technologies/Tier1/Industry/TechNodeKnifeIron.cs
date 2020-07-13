namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.Industry
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeKnifeIron : TechNode<TechGroupIndustryT1>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeKnifeIron>();

            config.SetRequiredNode<TechNodeMaceCopper>();
        }
    }
}