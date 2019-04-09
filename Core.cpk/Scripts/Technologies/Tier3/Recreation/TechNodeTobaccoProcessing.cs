namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Recreation
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes.DryingCabinet;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;

    public class TechNodeTobaccoProcessing : TechNode<TechGroupRecreation>
    {
        public override string Name => "Tobacco processing";

        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeTobaccoDried>()
                  .AddStructure<ObjectDryingCabinet>();
        }
    }
}