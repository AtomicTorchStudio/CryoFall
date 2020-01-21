namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Offense3
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeAmmo10mmStandard : TechNode<TechGroupOffense3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeAmmo10mmStandard>()
                  .AddRecipe<RecipeAmmo10mmBlank>();

            config.SetRequiredNode<TechNodeWeaponComponents>();
        }
    }
}