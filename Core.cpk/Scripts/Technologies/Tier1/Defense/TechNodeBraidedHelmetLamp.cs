namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.Defense
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeBraidedHelmetLamp : TechNode<TechGroupDefense>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeBraidedHelmetLamp>();

            config.SetRequiredNode<TechNodeBraidedArmor>();
        }
    }
}