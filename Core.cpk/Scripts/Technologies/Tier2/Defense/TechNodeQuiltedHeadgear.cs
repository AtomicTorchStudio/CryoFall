namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Defense
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeQuiltedHeadgear : TechNode<TechGroupDefenseT2>
    {
        public override string Name => "Quilted headgear";

        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeQuiltedHelmetHat>()
                  .AddRecipe<RecipeQuiltedHelmetPadded>();

            config.SetRequiredNode<TechNodeQuiltedArmor>();
        }
    }
}