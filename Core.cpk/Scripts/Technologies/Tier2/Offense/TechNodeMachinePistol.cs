namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Offense
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeMachinePistol : TechNode<TechGroupOffenseT2>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeMachinePistol>();

            config.SetRequiredNode<TechNodeRevolver8mm>();
        }
    }
}