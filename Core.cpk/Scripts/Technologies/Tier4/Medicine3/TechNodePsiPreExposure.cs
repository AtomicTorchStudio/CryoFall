namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Medicine3
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodePsiPreExposure : TechNode<TechGroupMedicine3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipePsiPreExposure>();

            config.SetRequiredNode<TechNodeAntiToxinPreExposure>();
        }
    }
}