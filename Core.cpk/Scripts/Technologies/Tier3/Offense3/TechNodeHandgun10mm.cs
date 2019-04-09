namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Offense3
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeHandgun10mm : TechNode<TechGroupOffense3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeHandgun10mm>();

            config.SetRequiredNode<TechNodeWeaponComponents>();
        }
    }
}