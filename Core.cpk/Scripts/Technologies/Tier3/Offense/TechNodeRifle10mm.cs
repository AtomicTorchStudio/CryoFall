namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Offense
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeRifle10mm : TechNode<TechGroupOffenseT3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeRifle10mm>();

            config.SetRequiredNode<TechNodeSubmachinegun10mm>();
        }
    }
}