namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.Farming
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeTomato : TechNode<TechGroupFarming>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeSeedsTomato>();

            config.SetRequiredNode<TechNodeCucumber>();
        }
    }
}