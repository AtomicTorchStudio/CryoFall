namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Cooking
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeJam : TechNode<TechGroupCookingT2>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeJamFromBerriesOrange>()
                  .AddRecipe<RecipeJamFromBerriesRed>()
                  .AddRecipe<RecipeJamFromBerriesViolet>();

            config.SetRequiredNode<TechNodeInsectMeatFried>();
        }
    }
}