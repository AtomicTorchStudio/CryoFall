namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Jewelry
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeGoldPants : TechNode<TechGroupJewelry>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeGoldPants>();

            config.SetRequiredNode<TechNodeGoldChestplate>();
        }
    }
}