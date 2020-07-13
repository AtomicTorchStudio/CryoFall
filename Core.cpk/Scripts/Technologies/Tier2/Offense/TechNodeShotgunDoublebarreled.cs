namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Offense
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeShotgunDoublebarreled : TechNode<TechGroupOffenseT2>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeShotgunDoublebarreled>();

            config.SetRequiredNode<TechNodeAmmo12gaBuckshot>();
        }
    }
}