namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Cooking
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes.DryingCabinet;

    public class TechNodeSpices : TechNode<TechGroupCookingT3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeSpices>();

            config.SetRequiredNode<TechNodeCarrotGrilled>();
        }
    }
}