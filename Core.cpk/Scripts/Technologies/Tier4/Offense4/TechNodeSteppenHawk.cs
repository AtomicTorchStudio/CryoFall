namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Offense4
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeSteppenHawk : TechNode<TechGroupOffense4>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeSteppenHawk>();

            config.SetRequiredNode<TechNodeAmmo50SH>();
        }
    }
}