namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Industry2
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeSteelSmelting : TechNode<TechGroupIndustry2>
    {
        public override string Name => "Steel smelting";

        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeIngotSteel>();

            config.SetRequiredNode<TechNodeAdvancedIndustry>();
        }
    }
}