namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.OffenseAndDefense
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeMusket : TechNode<TechGroupOffenseAndDefense>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeMusket>();

            config.SetRequiredNode<TechNodePaperCartridge>();
        }
    }
}