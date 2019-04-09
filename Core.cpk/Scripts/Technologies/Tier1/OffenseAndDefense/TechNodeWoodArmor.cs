namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.OffenseAndDefense
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;

    public class TechNodeWoodArmor : TechNode<TechGroupOffenseAndDefense>
    {
        public override string Name => "Wooden armor";

        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeWoodChestplate>()
                  .AddRecipe<RecipeWoodHelmet>()
                  .AddRecipe<RecipeWoodPants>()
                  .AddStructure<ObjectArmorerWorkbench>();

            config.SetRequiredNode<TechNodeGlue>();
        }
    }
}