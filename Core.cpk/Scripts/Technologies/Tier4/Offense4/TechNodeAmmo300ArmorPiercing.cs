namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Offense4
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeAmmo300ArmorPiercing : TechNode<TechGroupOffense4>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeAmmo300ArmorPiercing>();

            //config.SetRequiredNode<TechNode300Caliber>();
        }
    }
}