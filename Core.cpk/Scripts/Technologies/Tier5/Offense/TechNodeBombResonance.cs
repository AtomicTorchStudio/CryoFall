namespace AtomicTorch.CBND.CoreMod.Technologies.Tier5.Offense
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeBombResonance : TechNode<TechGroupOffenseT5>
    {
        public override FeatureAvailability AvailableIn => FeatureAvailability.OnlyPvP;

        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeBombResonance>();

            config.SetRequiredNode<TechNodeRifle300>();
        }
    }
}