namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Industry
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes.MineralProcessingPlant;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;

    public class TechNodeMineralProcessingPlant : TechNode<TechGroupIndustryT4>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectMineralProcessingPlant>()
                  .AddRecipe<RecipeSandFromStone>();

            config.SetRequiredNode<TechNodeOilCrackingPlant>();
        }
    }
}