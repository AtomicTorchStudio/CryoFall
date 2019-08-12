namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.Offense
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodePaperCartridge : TechNode<TechGroupOffense>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeAmmoPaperCartridge>();

            config.SetRequiredNode<TechNodeBlackpowder>();
        }
    }
}