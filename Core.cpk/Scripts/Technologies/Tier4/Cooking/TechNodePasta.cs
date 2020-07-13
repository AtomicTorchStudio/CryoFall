namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Cooking
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodePasta : TechNode<TechGroupCookingT4>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipePasta>();

            config.SetRequiredNode<TechNodeChiliBeans>();
        }
    }
}