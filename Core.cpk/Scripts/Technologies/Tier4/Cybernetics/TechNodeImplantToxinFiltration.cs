namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Cybernetics
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeImplantToxinFiltration : TechNode<TechGroupCyberneticsT4>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeImplantToxinFiltration>();

            config.SetRequiredNode<TechNodeImplantHealingGland>();
        }
    }
}