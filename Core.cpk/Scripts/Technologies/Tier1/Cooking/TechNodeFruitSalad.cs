namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.Cooking
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeFruitSalad : TechNode<TechGroupCookingT1>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects.AddRecipe<RecipeSaladFruit>();

            config.SetRequiredNode<TechNodeCookingTable>();
        }
    }
}