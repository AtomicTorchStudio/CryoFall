namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Industry
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeDroneIndustrialStandard : TechNode<TechGroupIndustryT3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeDroneIndustrialStandard>();

            config.SetRequiredNode<TechNodeDroneControlStandard>();
        }
    }
}