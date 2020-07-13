namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.Cooking
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes.DryingCabinet;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;

    public class TechNodeDryingCabinet : TechNode<TechGroupCookingT1>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeMeatJerky>()
                  .AddStructure<ObjectDryingCabinet>();

            config.SetRequiredNode<TechNodeVegetableSalad>();
        }
    }
}