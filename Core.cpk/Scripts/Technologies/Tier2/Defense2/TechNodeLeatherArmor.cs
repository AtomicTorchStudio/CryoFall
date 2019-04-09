namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Defense2
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeLeatherArmor : TechNode<TechGroupDefense2>
    {
        public override string Name => "Leather armor";

        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeLeatherJacket>()
                  .AddRecipe<RecipeLeatherPants>();

            config.SetRequiredNode<TechNodeQuiltedHeadgear>();
        }
    }
}