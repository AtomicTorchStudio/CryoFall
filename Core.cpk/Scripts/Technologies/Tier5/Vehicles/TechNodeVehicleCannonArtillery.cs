namespace AtomicTorch.CBND.CoreMod.Technologies.Tier5.Vehicles
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeVehicleCannonArtillery : TechNode<TechGroupVehiclesT5>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeVehicleCannonArtillery>();

            config.SetRequiredNode<TechNodeBehemoth>();
        }
    }
}