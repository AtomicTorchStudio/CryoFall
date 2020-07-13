namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.Industry
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeIronTools : TechNode<TechGroupIndustryT1>
    {
        public override string Name => "Iron tools";

        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeAxeIron>()
                  .AddRecipe<RecipePickaxeIron>()
                  .AddRecipe<RecipeToolboxT2>();

            config.SetRequiredNode<TechNodeSmelting>();
        }
    }
}