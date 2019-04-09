namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Offense2
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeAmmo8mmToxic : TechNode<TechGroupOffense2>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeAmmo8mmToxic>();

            config.SetRequiredNode<TechNodeAmmo8mmStandard>();
        }
    }
}