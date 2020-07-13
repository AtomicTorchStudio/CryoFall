namespace AtomicTorch.CBND.CoreMod.Technologies.Tier5.Offense
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeAmmoGrenadeFreeze : TechNode<TechGroupOffenseT5>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeAmmoGrenadeFreeze>();

            config.SetRequiredNode<TechNodeAmmo300Incendiary>();
        }
    }
}