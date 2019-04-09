namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Offense3
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeWeaponComponents : TechNode<TechGroupOffense3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeComponentsWeapon>();
        }
    }
}