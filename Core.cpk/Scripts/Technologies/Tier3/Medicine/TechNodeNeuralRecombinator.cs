namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Medicine
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeNeuralRecombinator : TechNode<TechGroupMedicineT3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeNeuralRecombinator>();

            config.SetRequiredNode<TechNodeStrengthBoostBig>();
        }
    }
}