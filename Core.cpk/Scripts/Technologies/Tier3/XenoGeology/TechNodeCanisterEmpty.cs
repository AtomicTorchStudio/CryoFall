namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.XenoGeology
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeCanisterEmpty : TechNode<TechGroupXenogeology>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeCanisterEmpty>();

            config.SetRequiredNode<TechNodeOilPump>();
        }
    }
}