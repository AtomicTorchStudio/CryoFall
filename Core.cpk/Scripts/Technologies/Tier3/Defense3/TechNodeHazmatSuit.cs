namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Defense3
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeHazmatSuit : TechNode<TechGroupDefense3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeHazmatSuit>();

            config.SetRequiredNode<TechNodeMetalHeadgear>();
        }
    }
}