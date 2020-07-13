namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Industry
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes.MineralProcessingPlant;

    public class TechNodeIronConcentrate : TechNode<TechGroupIndustryT4>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeOreIronConcentrate>();

            config.SetRequiredNode<TechNodeMineralProcessingPlant>();
        }
    }
}