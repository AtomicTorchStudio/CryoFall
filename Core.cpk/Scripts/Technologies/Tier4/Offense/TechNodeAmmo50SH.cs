namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Offense
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeAmmo50SH : TechNode<TechGroupOffenseT4>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeAmmo50SH>();

            config.SetRequiredNode<TechNodeAmmo300ArmorPiercing>();
        }
    }
}