namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Industry2
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeCement : TechNode<TechGroupIndustry2>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeCement>();

            //config.SetRequiredNode<TechNodeAdvancedIndustry>();
        }
    }
}