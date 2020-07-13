namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Offense
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeSubmachinegun10mm : TechNode<TechGroupOffenseT3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeSubmachinegun10mm>();

            config.SetRequiredNode<TechNodeHandgun10mm>();
        }
    }
}