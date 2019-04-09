namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Defense3
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeHelmetNightVision : TechNode<TechGroupDefense3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeHelmetNightVision>();

            config.SetRequiredNode<TechNodeMilitaryArmor>();
        }
    }
}