namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Chemistry
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeAcidSulfuricFromPyrite : TechNode<TechGroupChemistry>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeAcidSulfuricFromPyrite>();

            config.SetRequiredNode<TechNodeChemicalLab>();
        }
    }
}