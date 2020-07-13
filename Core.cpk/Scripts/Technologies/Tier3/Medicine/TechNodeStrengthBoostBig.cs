namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Medicine
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeStrengthBoostBig : TechNode<TechGroupMedicineT3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeStrengthBoostBig>();

            config.SetRequiredNode<TechNodeMedkit>();
        }
    }
}