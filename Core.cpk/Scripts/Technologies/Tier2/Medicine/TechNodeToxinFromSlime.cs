namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Medicine
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeToxinFromSlime : TechNode<TechGroupMedicineT2>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeToxinFromSlime>();

            config.SetRequiredNode<TechNodePainkiller>();
        }
    }
}