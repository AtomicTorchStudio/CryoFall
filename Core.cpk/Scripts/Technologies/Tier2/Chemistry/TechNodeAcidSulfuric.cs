namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Chemistry
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeAcidSulfuric : TechNode<TechGroupChemistryT2>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeAcidSulfuric>();

            config.SetRequiredNode<TechNodeChemicalLab>();
        }
    }
}