namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.Cooking
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeSalt : TechNode<TechGroupCooking>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeSalt>();

            config.SetRequiredNode<TechNodeRoastedMeat>();
        }
    }
}