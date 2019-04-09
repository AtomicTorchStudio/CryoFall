namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Cooking2
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodePie : TechNode<TechGroupCooking2>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipePieBerry>();

            config.SetRequiredNode<TechNodeSandwich>();
        }
    }
}