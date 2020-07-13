namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Jewelry
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeGoldCrown : TechNode<TechGroupJewelryT4>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeGoldHelmet>();

            config.SetRequiredNode<TechNodeGoldArmor>();
        }
    }
}