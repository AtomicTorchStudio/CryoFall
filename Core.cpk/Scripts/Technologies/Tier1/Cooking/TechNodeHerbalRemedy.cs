namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.Cooking
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeHerbalRemedy : TechNode<TechGroupCooking>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeHerbalRemedy>();

            config.SetRequiredNode<TechNodeCookingTable>();
        }
    }
}