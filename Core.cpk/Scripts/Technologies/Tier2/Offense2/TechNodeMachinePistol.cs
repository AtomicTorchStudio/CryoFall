namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Offense2
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeMachinePistol : TechNode<TechGroupOffense2>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeMachinePistol>();

            config.SetRequiredNode<TechNodeLuger>();
        }
    }
}