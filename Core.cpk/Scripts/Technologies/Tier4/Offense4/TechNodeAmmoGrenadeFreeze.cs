namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Offense4
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeAmmoGrenadeFreeze : TechNode<TechGroupOffense4>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeAmmoGrenadeFreeze>();

            config.SetRequiredNode<TechNodeAmmoGrenadeIncendiary>();
        }
    }
}