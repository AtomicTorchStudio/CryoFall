namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.Offense
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeAmmoArrowStone : TechNode<TechGroupOffense>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeAmmoArrowStone>();

            config.SetRequiredNode<TechNodeCrossbow>();
        }
    }
}