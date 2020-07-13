namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.Offense
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeAmmoArrowIron : TechNode<TechGroupOffenseT1>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeAmmoArrowIron>();

            config.SetRequiredNode<TechNodeAmmoArrowStone>();
        }
    }
}