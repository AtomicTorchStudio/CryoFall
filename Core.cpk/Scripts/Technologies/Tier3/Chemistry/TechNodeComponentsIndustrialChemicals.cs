namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Chemistry
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeComponentsIndustrialChemicals : TechNode<TechGroupChemistryT3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeComponentsIndustrialChemicals>();

            config.SetRequiredNode<TechNodePlastic>();
        }
    }
}