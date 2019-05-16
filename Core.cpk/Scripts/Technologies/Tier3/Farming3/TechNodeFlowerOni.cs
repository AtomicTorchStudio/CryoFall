namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Farming3
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Farms;

    public class TechNodeFlowerOni : TechNode<TechGroupFarming3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeSeedsFlowerOni>();

            config.SetRequiredNode<TechNodePlantPot>();
        }
    }
}