namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Electricity3
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeSolarPanel : TechNode<TechGroupElectricity3>
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