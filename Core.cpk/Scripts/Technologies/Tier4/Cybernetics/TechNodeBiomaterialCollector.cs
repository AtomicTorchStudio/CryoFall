namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Cybernetics
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeBiomaterialCollector : TechNode<TechGroupCybernetics>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeBiomaterialCollector>();

            config.SetRequiredNode<TechNodeVialEmpty>();
        }
    }
}