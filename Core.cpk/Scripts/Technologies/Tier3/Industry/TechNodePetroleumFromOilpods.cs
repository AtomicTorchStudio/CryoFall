namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Industry
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodePetroleumFromOilpods : TechNode<TechGroupIndustryT3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeCanisterPetroleum>();

            config.SetRequiredNode<TechNodeOilRefinery>();
        }
    }
}