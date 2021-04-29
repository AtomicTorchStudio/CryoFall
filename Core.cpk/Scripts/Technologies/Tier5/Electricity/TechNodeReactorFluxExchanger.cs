namespace AtomicTorch.CBND.CoreMod.Technologies.Tier5.Electricity
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeReactorFluxExchanger : TechNode<TechGroupElectricityT5>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeReactorFluxExchanger>();

            config.SetRequiredNode<TechNodeReactorFuelRod>();
        }
    }
}