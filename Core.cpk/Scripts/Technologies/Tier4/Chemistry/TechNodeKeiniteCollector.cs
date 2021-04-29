namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Chemistry
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeKeiniteCollector : TechNode<TechGroupChemistryT4>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeKeiniteCollector>();

            config.SetRequiredNode<TechNodeSolvent>();
        }
    }
}