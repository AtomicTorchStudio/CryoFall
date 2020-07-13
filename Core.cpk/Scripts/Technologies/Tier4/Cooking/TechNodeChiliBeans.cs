namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Cooking
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeChiliBeans : TechNode<TechGroupCookingT4>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeChiliBeans>();

            //config.SetRequiredNode<TechNodeStew>();
        }
    }
}