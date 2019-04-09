namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.Farming
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeBellPepper : TechNode<TechGroupFarming>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeSeedsBellPepper>();

            config.SetRequiredNode<TechNodeTomato>();
        }
    }
}