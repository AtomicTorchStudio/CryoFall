namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.Cooking
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeJam : TechNode<TechGroupCooking>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeJamBerriesOrange>()
                  .AddRecipe<RecipeJamBerriesRed>()
                  .AddRecipe<RecipeJamBerriesViolet>();

            config.SetRequiredNode<TechNodeStove>();
        }
    }
}