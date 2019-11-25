namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Vehicles
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeImpulseEngine : TechNode<TechGroupVehicles>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                .AddRecipe<RecipeImpulseEngine>();

            config.SetRequiredNode<TechNodeVehicleAssemblyBay>();
        }
    }
}