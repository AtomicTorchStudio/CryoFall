namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Recreation
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes.FermentationBarrel;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;

    public class TechNodeFermentationBarrel : TechNode<TechGroupRecreation>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectFermentationBarrel>()
                  .AddRecipe<RecipeWine>();
        }
    }
}