namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Electricity
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeWireFromRubber : TechNode<TechGroupElectricityT2>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeWireFromRubber>();
        }
    }
}