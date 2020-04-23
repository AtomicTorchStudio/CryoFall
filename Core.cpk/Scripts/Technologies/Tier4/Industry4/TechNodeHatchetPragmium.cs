namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Industry4
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeHatchetPragmium : TechNode<TechGroupIndustry4>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeHatchetPragmium>();

            config.SetRequiredNode<TechNodeBatteryHeavyDuty>();
        }
    }
}