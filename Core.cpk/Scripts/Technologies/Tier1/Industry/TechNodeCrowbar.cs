namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.Industry
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeCrowbar : TechNode<TechGroupIndustry>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeCrowbar>();

            config.SetRequiredNode<TechNodeIronTools>();
        }
    }
}