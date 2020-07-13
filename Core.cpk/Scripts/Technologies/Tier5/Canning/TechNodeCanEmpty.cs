namespace AtomicTorch.CBND.CoreMod.Technologies.Tier5.Canning
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeCanEmpty : TechNode<TechGroupCanningT5>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeCanEmpty>();
        }
    }
}