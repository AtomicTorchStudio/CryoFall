namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Vehicles
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeVehicleAutocannonLight : TechNode<TechGroupVehiclesT4>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeVehicleAutocannonLight>();

            config.SetRequiredNode<TechNodeUniversalActuator>();
        }
    }
}