namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Vehicles2
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeVehicleAutocannonLight : TechNode<TechGroupVehicles2>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                .AddRecipe<RecipeVehicleAutocannonLight>();

            config.SetRequiredNode<TechNodeUniversalActuator>();
        }
    }
}