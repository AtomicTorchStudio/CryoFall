namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.Offense
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeMusket : TechNode<TechGroupOffenseT1>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeMusket>();

            config.SetRequiredNode<TechNodePaperCartridge>();
        }
    }
}