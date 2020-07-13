namespace AtomicTorch.CBND.CoreMod.Technologies.Tier5.Offense
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeRifle300 : TechNode<TechGroupOffenseT5>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeRifle300>();

            config.SetRequiredNode<TechNodeAmmo300Incendiary>();
        }
    }
}