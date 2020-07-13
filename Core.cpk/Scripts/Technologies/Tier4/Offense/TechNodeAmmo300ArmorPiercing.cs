namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Offense
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeAmmo300ArmorPiercing : TechNode<TechGroupOffenseT4>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeAmmo300ArmorPiercing>();

            //config.SetRequiredNode<TechNode300Caliber>();
        }
    }
}