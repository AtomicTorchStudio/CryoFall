namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Jewelry
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeGoldChestplate : TechNode<TechGroupJewelry>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeGoldChestplate>();
        }
    }
}