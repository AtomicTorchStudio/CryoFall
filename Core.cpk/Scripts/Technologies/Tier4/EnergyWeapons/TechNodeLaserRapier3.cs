namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.EnergyWeapons
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeLaserRapier3 : TechNode<TechGroupEnergyWeapons>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeRapierLaserGreen>()
                  .AddRecipe<RecipeRapierLaserRed>()
                  .AddRecipe<RecipeRapierLaserBlue>();

            config.SetRequiredNode<TechNodeLaserRapier2>();
        }
    }
}