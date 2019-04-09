namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.XenoGeology
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeIngotLithium : TechNode<TechGroupXenogeology>
    {
        public override string Name => "Lithium refinement";

        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeIngotLithium>();

            config.SetRequiredNode<TechNodeLithiumOreExtractor>();
        }
    }
}