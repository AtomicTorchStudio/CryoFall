namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Recreation
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes.DryingCabinet;

    public class TechNodeTobaccoAged : TechNode<TechGroupRecreationT3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeTobaccoAged>();

            config.SetRequiredNode<TechNodeCigarCheap>();
        }
    }
}