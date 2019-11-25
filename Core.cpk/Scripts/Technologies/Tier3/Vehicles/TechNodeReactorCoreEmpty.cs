namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Vehicles
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeReactorCoreEmpty : TechNode<TechGroupVehicles>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeReactorCoreEmpty>();

            config.SetRequiredNode<TechNodeVehicleAssemblyBay>();
        }
    }
}