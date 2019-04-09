namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.OffenseAndDefense
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeThread : TechNode<TechGroupOffenseAndDefense>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeThread>();
        }
    }
}