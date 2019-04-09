namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.Cooking
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeFruitSalad : TechNode<TechGroupCooking>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects.AddRecipe<RecipeSaladFruit>();

            config.SetRequiredNode<TechNodeCookingTable>();
        }
    }
}