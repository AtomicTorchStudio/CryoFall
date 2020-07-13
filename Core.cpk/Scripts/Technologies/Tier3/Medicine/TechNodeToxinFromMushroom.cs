namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Medicine
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeToxinFromMushroom : TechNode<TechGroupMedicineT3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeToxinFromMushroom>();

            config.SetRequiredNode<TechNodeHemostatic>();
        }
    }
}