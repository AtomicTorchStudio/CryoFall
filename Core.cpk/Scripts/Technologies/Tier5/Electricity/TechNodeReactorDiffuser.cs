namespace AtomicTorch.CBND.CoreMod.Technologies.Tier5.Electricity
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeReactorDiffuser : TechNode<TechGroupElectricityT5>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeReactorDiffuser>();

            config.SetRequiredNode<TechNodeReactorDeflector>();
        }
    }
}