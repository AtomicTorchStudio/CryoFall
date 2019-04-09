namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Commerce
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeCoinRecycling : TechNode<TechGroupCommerce>
    {
        public override string Name => "Coin recycling";

        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeCoinPennyRecycle>()
                  .AddRecipe<RecipeCoinShinyRecycle>();

            config.SetRequiredNode<TechNodeCoinMinting>();
        }
    }
}