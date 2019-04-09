namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.Farming
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeCucumber : TechNode<TechGroupFarming>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeSeedsCucumber>();

            config.SetRequiredNode<TechNodeFarmingBasics>();
        }
    }
}