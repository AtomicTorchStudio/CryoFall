namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Cooking2
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeCactusDrink : TechNode<TechGroupCooking2>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeCactusDrink>();

            config.SetRequiredNode<TechNodeFriedYucca>();
        }
    }
}