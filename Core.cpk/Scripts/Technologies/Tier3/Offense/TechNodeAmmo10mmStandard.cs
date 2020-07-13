namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Offense
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeAmmo10mmStandard : TechNode<TechGroupOffenseT3>
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