namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Medicine2
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeStrengthBoostBig : TechNode<TechGroupMedicine2>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeStrengthBoostBig>();

            config.SetRequiredNode<TechNodeHemostatic>();
        }
    }
}