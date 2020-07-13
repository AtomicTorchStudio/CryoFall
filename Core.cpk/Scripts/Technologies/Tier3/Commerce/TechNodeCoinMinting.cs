namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Commerce
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;

    public class TechNodeCoinMinting : TechNode<TechGroupCommerceT3>
    {
        public override string Name => "Coin minting";

        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectCoinMint>()
                  .AddRecipe<RecipeCoinPenny>()
                  .AddRecipe<RecipeCoinShiny>();
        }
    }
}