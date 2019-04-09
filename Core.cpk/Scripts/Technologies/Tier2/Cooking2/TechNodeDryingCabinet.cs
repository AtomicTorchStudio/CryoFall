namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Cooking2
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes.DryingCabinet;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;

    public class TechNodeDryingCabinet : TechNode<TechGroupCooking2>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeMeatJerky>()
                  .AddStructure<ObjectDryingCabinet>();

            config.SetRequiredNode<TechNodeCarrotGrilled>();
        }
    }
}