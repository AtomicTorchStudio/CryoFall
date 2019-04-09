namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Offense3
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeAmmo10mmHollowPoint : TechNode<TechGroupOffense3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeAmmo10mmHollowPoint>();

            config.SetRequiredNode<TechNodeAmmo10mmArmorPiercing>();
        }
    }
}