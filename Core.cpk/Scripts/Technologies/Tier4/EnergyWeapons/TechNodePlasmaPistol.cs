namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.EnergyWeapons
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodePlasmaPistol : TechNode<TechGroupEnergyWeapons>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipePlasmaPistol>();

            config.SetRequiredNode<TechNodePlasmaWeapons>();
        }
    }
}