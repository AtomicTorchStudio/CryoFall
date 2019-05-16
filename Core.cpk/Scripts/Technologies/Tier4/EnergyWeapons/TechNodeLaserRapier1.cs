namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.EnergyWeapons
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeLaserRapier1 : TechNode<TechGroupEnergyWeapons>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeRapierLaserWhite>();

            config.SetRequiredNode<TechNodeLaserPistol>();
        }
    }
}