namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Cooking2
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeCarrotGrilled : TechNode<TechGroupCooking2>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeCarrotGrilled>();

            config.SetRequiredNode<TechNodeCornOnCob>();
        }
    }
}