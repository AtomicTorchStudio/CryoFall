namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Vehicles2
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeVehicleAutocannonHeavy : TechNode<TechGroupVehicles2>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                .AddRecipe<RecipeVehicleAutocannonHeavy>();

            config.SetRequiredNode<TechNodeVehicleAutocannonLight>();
        }
    }
}