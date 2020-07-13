namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Offense
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeAmmoGrenadeFragmentation : TechNode<TechGroupOffenseT4>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeAmmoGrenadeFragmentation>();

            config.SetRequiredNode<TechNodeAmmoGrenadeIncendiary>();
        }
    }
}