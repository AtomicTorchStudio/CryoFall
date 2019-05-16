namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Farming2
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeCorn : TechNode<TechGroupFarming2>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeSeedsCorn>();

            config.SetRequiredNode<TechNodeWateringCanSteel>();
        }
    }
}