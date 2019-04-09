namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.Industry
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeKnifeIron : TechNode<TechGroupIndustry>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeKnifeIron>();

            config.SetRequiredNode<TechNodeIronTools>();
        }
    }
}