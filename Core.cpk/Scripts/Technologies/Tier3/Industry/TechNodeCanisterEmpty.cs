namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Industry
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.GameApi.Resources;

    public class TechNodeCanisterEmpty : TechNode<TechGroupIndustryT3>
    {
        public override ITextureResource Icon
            => GetProtoEntity<ItemCanisterEmpty>().Icon;

        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeCanisterEmptyFromMetal>()
                  .AddRecipe<RecipeCanisterEmptyFromPlastic>();

            config.SetRequiredNode<TechNodeIngotLithium>();
        }
    }
}