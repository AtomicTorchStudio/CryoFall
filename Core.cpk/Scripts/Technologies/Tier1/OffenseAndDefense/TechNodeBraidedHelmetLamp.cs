namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.OffenseAndDefense
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeBraidedHelmetLamp : TechNode<TechGroupOffenseAndDefense>
    {

        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeBraidedHelmetLamp>();

            config.SetRequiredNode<TechNodeBraidedArmor>();
        }
    }
}