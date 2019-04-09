namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.EnergyWeapons
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodePlasmaRifle : TechNode<TechGroupEnergyWeapons>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipePlasmaRifle>();

            config.SetRequiredNode<TechNodePlasmaPistol>();
        }
    }
}