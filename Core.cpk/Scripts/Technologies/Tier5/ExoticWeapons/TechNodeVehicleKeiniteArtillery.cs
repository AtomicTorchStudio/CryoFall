namespace AtomicTorch.CBND.CoreMod.Technologies.Tier5.ExoticWeapons
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeVehicleKeiniteArtillery : TechNode<TechGroupExoticWeaponsT5>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeVehicleKeiniteArtillery>();

            config.SetRequiredNode<TechNodeNemesis>();
        }
    }
}