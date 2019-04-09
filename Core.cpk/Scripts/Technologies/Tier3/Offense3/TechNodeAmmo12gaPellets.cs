namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Offense3
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeAmmo12gaPellets : TechNode<TechGroupOffense3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeAmmo12gaPellets>();

            config.SetRequiredNode<TechNodeAmmo12gaSlugs>();
        }
    }
}