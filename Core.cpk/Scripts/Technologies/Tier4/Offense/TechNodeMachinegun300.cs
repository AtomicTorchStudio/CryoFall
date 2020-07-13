namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Offense
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeMachinegun300 : TechNode<TechGroupOffenseT4>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeMachinegun300>();

            config.SetRequiredNode<TechNodeAmmo300ArmorPiercing>();
        }
    }
}