namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Farming
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeChiliPepper : TechNode<TechGroupFarmingT2>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeSeedsChiliPepper>();

            config.SetRequiredNode<TechNodeCorn>();
        }
    }
}