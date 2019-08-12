namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Cooking3
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeCarrotGrilled : TechNode<TechGroupCooking3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeCarrotGrilled>();
        }
    }
}