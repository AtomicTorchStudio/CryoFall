namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.OffenseAndDefense
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;

    public class TechNodeBraidedArmor : TechNode<TechGroupOffenseAndDefense>
    {
        public override string Name => "Braided armor";

        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeBraidedChestplate>()
                  .AddRecipe<RecipeBraidedHelmet>()
                  .AddRecipe<RecipeBraidedPants>()
                  .AddStructure<ObjectArmorerWorkbench>();

            config.SetRequiredNode<TechNodeGlue>();
        }
    }
}