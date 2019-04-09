namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Industry3
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeComponentsOptical : TechNode<TechGroupIndustry3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeComponentsOptical>();

            config.SetRequiredNode<TechNodeComponentsElectronic>();
        }
    }
}