namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Offense
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeLaserSight : TechNode<TechGroupOffenseT2>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeLaserSight>();

            config.SetRequiredNode<TechNodeAmmo12gaBuckshot>();
        }
    }
}