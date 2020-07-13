namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Cooking
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes.DryingCabinet;

    public class TechNodeSalami : TechNode<TechGroupCookingT3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeSalami>();

            config.SetRequiredNode<TechNodeCactusDrink>();
        }
    }
}