namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Offense2
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeRevolver8mm : TechNode<TechGroupOffense2>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeRevolver8mm>();

            config.SetRequiredNode<TechNodeAmmo8mmStandard>();
        }
    }
}