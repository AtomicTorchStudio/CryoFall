namespace AtomicTorch.CBND.CoreMod.Technologies.Tier5.ExoticWeapons
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeSwarmLauncher : TechNode<TechGroupExoticWeaponsT5>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeSwarmLauncher>();

            config.SetRequiredNode<TechNodeToxinProliferator>();
        }
    }
}