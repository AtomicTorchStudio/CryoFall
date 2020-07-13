namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.Industry
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodePaper : TechNode<TechGroupIndustryT1>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipePaper>();

            config.SetRequiredNode<TechNodeGlassware>();
        }
    }
}