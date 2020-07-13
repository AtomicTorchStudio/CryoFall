namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Cooking
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes.DryingCabinet;

    public class TechNodeFishDried : TechNode<TechGroupCookingT2>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeFishDried>();

            config.SetRequiredNode<TechNodeJam>();
        }
    }
}