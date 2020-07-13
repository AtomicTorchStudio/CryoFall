namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Cooking
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeWheatFlour : TechNode<TechGroupCookingT2>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeWheatFlour>();

            config.SetRequiredNode<TechNodeInsectMeatFried>();
        }
    }
}