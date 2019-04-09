namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Chemistry
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeNitrocellulosePowder : TechNode<TechGroupChemistry>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeNitrocellulosePowder>();

            config.SetRequiredNode<TechNodeAcidNitric>();
        }
    }
}