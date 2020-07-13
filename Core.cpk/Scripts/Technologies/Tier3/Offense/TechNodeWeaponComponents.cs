namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Offense
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeWeaponComponents : TechNode<TechGroupOffenseT3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeComponentsWeapon>();
        }
    }
}