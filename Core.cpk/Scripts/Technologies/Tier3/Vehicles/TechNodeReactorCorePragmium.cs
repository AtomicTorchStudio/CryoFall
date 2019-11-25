namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Vehicles
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeReactorCorePragmium : TechNode<TechGroupVehicles>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeReactorCorePragmium>();

            config.SetRequiredNode<TechNodeReactorCoreEmpty>();
        }
    }
}