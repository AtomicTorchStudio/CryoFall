namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Medicine
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodePsiPreExposure : TechNode<TechGroupMedicineT4>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipePsiPreExposure>();

            config.SetRequiredNode<TechNodeStimpack>();
        }
    }
}