namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Vehicles2
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc;
    using AtomicTorch.CBND.CoreMod.Vehicles;

    public class TechNodeUniversalActuator : TechNode<TechGroupVehicles2>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                .AddRecipe<RecipeUniversalActuator>();

            //config.SetRequiredNode<TechNodeStructuralPlating>();
        }
    }
}