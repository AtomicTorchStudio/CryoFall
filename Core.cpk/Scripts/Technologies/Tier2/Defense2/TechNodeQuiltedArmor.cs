namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Defense2
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeQuiltedArmor : TechNode<TechGroupDefense2>
    {
        public override string Name => "Quilted armor";

        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeQuiltedCoat>()
                  .AddRecipe<RecipeQuiltedPants>();
        }
    }
}