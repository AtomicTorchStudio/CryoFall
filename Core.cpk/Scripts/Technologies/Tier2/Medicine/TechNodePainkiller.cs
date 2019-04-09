namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Medicine
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodePainkiller : TechNode<TechGroupMedicine>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipePainkiller>();

            config.SetRequiredNode<TechNodeBandage>();
        }
    }
}