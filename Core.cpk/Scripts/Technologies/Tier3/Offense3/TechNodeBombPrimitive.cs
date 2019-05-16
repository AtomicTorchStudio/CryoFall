namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Offense3
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeBombPrimitive : TechNode<TechGroupOffense3>
    {
        public override FeatureAvailability AvailableIn => FeatureAvailability.OnlyPvP;

        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeBombPrimitive>();

            config.SetRequiredNode<TechNodeShotgunMilitary>();
        }
    }
}