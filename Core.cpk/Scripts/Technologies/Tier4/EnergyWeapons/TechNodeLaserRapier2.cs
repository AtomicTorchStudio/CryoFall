namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.EnergyWeapons
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeLaserRapier2 : TechNode<TechGroupEnergyWeaponsT4>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeRapierLaserRed>()
                  .AddRecipe<RecipeRapierLaserGreen>()
                  .AddRecipe<RecipeRapierLaserBlue>();

            config.SetRequiredNode<TechNodeLaserRapier1>();
        }
    }
}