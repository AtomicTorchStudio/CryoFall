namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Industry4
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeBatteryHeavyDuty : TechNode<TechGroupIndustry4>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeBatteryHeavyDuty>();

            config.SetRequiredNode<TechNodeComponentsHighTech>();
        }
    }
}