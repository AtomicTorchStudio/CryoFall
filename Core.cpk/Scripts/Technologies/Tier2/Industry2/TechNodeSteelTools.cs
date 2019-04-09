namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Industry2
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeSteelTools : TechNode<TechGroupIndustry2>
    {
        public override string Name => "Steel tools";

        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeAxeSteel>()
                  .AddRecipe<RecipePickaxeSteel>()
                  .AddRecipe<RecipeToolboxT3>();

            config.SetRequiredNode<TechNodeSteelSmelting>();
        }
    }
}