namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Recreation
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeCigarCheap : TechNode<TechGroupRecreationT3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeCigarCheap>();

            config.SetRequiredNode<TechNodeTobaccoProcessing>();
        }
    }
}