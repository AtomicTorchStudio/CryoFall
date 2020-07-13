namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Offense
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeBombModern : TechNode<TechGroupOffenseT4>
    {
        public override FeatureAvailability AvailableIn => FeatureAvailability.OnlyPvP;

        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeBombModern>();

            config.SetRequiredNode<TechNodeMachinegun300>();
        }
    }
}