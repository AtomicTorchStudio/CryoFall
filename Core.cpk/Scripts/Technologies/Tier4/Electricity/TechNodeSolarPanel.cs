namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Electricity
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeSolarPanel : TechNode<TechGroupElectricityT4>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeSolarPanel>()
                  .AddRecipe<RecipeSolarPanelFromBrokenPanels>();

            config.SetRequiredNode<TechNodeGeneratorSolar>();
        }
    }
}