namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.Industry
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;

    public class TechNodeSmelting : TechNode<TechGroupIndustry>
    {
        public override string Name => "Smelting";

        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectFurnace>()
                  .AddRecipe<RecipeIngotCopperFromOre>()
                  .AddRecipe<RecipeIngotCopperFromConcentrate>()
                  .AddRecipe<RecipeIngotIronFromOre>()
                  .AddRecipe<RecipeIngotIronFromConcentrate>();

            config.SetRequiredNode<TechNodeWorkbench>();
        }
    }
}