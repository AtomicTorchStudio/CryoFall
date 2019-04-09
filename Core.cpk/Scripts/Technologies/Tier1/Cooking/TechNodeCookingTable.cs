namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.Cooking
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;

    public class TechNodeCookingTable : TechNode<TechGroupCooking>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectCookingTable>()
                  .AddRecipe<RecipeMREImprovised>();
        }
    }
}