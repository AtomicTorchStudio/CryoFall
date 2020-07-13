namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.Industry
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeOilLamp : TechNode<TechGroupIndustryT1>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeOilLamp>()
                  .AddRecipe<RecipeCampFuelFromFat>();

            config.SetRequiredNode<TechNodeGlassware>();
        }
    }
}