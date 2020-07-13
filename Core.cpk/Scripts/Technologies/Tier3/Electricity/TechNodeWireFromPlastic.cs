namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Electricity
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeWireFromPlastic : TechNode<TechGroupElectricityT3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeWireFromPlastic>();
        }
    }
}