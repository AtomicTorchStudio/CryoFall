namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Offense
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeAmmo12gaBuckshot : TechNode<TechGroupOffenseT2>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeAmmo12gaBuckshot>()
                  .AddRecipe<RecipeAmmo12gaSaltCharge>();

            config.SetRequiredNode<TechNodeAmmo8mmToxic>();
        }
    }
}