namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Farming3
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeTobacco : TechNode<TechGroupFarming3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeSeedsTobacco>();

            config.SetRequiredNode<TechNodeSpecializedCrops>();
        }
    }
}