namespace AtomicTorch.CBND.CoreMod.Technologies.Tier5.Electricity
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeReactorFuelRod : TechNode<TechGroupElectricityT5>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeReactorFuelRod>()
                  .AddRecipe<RecipeReactorFuelRodEmpty>()
                  .AddRecipe<RecipeReactorBrokenModuleRecycle>();

            config.SetRequiredNode<TechNodeGeneratorPragmiumReactor>();
        }
    }
}