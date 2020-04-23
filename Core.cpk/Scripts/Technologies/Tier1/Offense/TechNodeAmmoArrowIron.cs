namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.Offense
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeAmmoArrowIron : TechNode<TechGroupOffense>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeAmmoArrowIron>();

            config.SetRequiredNode<TechNodeAmmoArrowStone>();
        }
    }
}