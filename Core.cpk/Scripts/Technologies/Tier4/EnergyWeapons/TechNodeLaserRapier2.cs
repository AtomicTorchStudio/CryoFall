namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.EnergyWeapons
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeLaserRapier2 : TechNode<TechGroupEnergyWeapons>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeRapierLaserPurple>()
                  .AddRecipe<RecipeRapierLaserYellow>();

            config.SetRequiredNode<TechNodeLaserRapier1>();
        }
    }
}