namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Farming
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeSpices : TechNode<TechGroupFarmingT3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeSeedsSpices>();
        }
    }
}