namespace AtomicTorch.CBND.CoreMod.Technologies.Tier5.Electricity
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeReactorModerator : TechNode<TechGroupElectricityT5>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeReactorModerator>();

            config.SetRequiredNode<TechNodeReactorFluxExchanger>();
        }
    }
}