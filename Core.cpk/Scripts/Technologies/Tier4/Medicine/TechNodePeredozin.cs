namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Medicine
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodePeredozin : TechNode<TechGroupMedicineT4>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipePeredozin>();

            config.SetRequiredNode<TechNodeHeatPreExposure>();
        }
    }
}