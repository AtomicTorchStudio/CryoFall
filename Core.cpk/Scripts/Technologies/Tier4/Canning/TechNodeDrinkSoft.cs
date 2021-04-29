namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Canning
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeDrinkSoft : TechNode<TechGroupCanningT4>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeDrinkSoft>();

            config.SetRequiredNode<TechNodePreservative>();
        }
    }
}