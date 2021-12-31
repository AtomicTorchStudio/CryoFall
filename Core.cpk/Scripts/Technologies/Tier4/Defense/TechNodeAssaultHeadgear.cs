namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Defense
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeAssaultHeadgear : TechNode<TechGroupDefenseT4>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeAssaultHelmet>();

            config.SetRequiredNode<TechNodeAssaultArmor>();
        }
    }
}