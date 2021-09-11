namespace AtomicTorch.CBND.CoreMod.Technologies.Tier5.ExoticWeapons
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeAmmoKeinite : TechNode<TechGroupExoticWeaponsT5>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeAmmoKeinite>();

            //config.SetRequiredNode<TechNodeReactorFuelRod>();
        }
    }
}