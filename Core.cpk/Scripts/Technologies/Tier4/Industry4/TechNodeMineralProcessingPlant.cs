namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Industry4
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;

    public class TechNodeMineralProcessingPlant : TechNode<TechGroupIndustry4>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectMineralProcessingPlant>()
                  .AddRecipe<RecipeSandFromStone>();

            config.SetRequiredNode<TechNodeComponentsHighTech>();
        }
    }
}