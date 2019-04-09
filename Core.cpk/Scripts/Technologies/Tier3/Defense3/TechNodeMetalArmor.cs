namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Defense3
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeMetalArmor : TechNode<TechGroupDefense3>
    {
        public override string Name => "Metal armor";

        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeMetalChestplate>()
                  .AddRecipe<RecipeMetalPants>();
        }
    }
}