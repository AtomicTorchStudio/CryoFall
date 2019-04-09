namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Defense2
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeQuiltedHeadgear : TechNode<TechGroupDefense2>
    {
        public override string Name => "Quilted headgear";

        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeQuiltedHat>()
                  .AddRecipe<RecipeQuiltedHelmet>();

            config.SetRequiredNode<TechNodeQuiltedArmor>();
        }
    }
}