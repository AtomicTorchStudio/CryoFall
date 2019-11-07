namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Farming3
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeHygroscopicGranules : TechNode<TechGroupFarming3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeHygroscopicGranules>();

            config.SetRequiredNode<TechNodeTobacco>();
        }
    }
}