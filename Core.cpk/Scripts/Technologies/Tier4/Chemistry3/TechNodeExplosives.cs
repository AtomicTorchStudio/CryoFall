namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Chemistry3
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeExplosives : TechNode<TechGroupChemistry3>
    {
        public override FeatureAvailability AvailableIn => FeatureAvailability.OnlyPvP;

        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeExplosives>();

            config.SetRequiredNode<TechNodeFormulatedGunpowder>();
        }
    }
}