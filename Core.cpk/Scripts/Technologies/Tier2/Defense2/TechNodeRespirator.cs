namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Defense2
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeRespirator : TechNode<TechGroupDefense2>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeRespirator>();

            config.SetRequiredNode<TechNodeHelmetMiner>();
        }
    }
}