namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Offense4
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeMachinegun300 : TechNode<TechGroupOffense4>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeMachinegun300>();

            config.SetRequiredNode<TechNode300Caliber>();
        }
    }
}