namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Industry
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeMachete : TechNode<TechGroupIndustryT2>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeMachete>();

            config.SetRequiredNode<TechNodeSteelTools>();
        }
    }
}