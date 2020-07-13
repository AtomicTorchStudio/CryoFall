namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Vehicles
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeVehicleAutocannonHeavy : TechNode<TechGroupVehiclesT4>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeVehicleAutocannonHeavy>();

            config.SetRequiredNode<TechNodeVehicleAutocannonLight>();
        }
    }
}