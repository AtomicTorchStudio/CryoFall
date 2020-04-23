namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.Industry
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeGlassware : TechNode<TechGroupIndustry>
    {
        public override string Name => "Glassware";

        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeGlass>()
                  .AddRecipe<RecipeBottle>()
                  .AddRecipe<RecipeBottleWaterFromWaterbulb>();

            config.SetRequiredNode<TechNodeSmelting>();
        }
    }
}