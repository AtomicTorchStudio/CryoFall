namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Chemistry
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeCanisterGasolineFromFuelSack : TechNode<TechGroupChemistryT3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeCanisterGasolineFromFuelSack>();

            config.SetRequiredNode<TechNodeComponentsPharmaceutical>();
        }
    }
}