namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Medicine
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeAntiToxinPreExposure : TechNode<TechGroupMedicineT3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeAntiToxinPreExposure>();

            config.SetRequiredNode<TechNodePlasterCast>();
        }
    }
}