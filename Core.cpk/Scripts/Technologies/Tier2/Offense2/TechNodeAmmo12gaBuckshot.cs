namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Offense2
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeAmmo12gaBuckshot : TechNode<TechGroupOffense2>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeAmmo12gaBuckshot>();

            config.SetRequiredNode<TechNodeAmmo8mmToxic>();
        }
    }
}