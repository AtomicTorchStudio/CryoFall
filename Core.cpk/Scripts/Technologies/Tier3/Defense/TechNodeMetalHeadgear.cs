namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Defense
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeMetalHeadgear : TechNode<TechGroupDefenseT3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeMetalHelmet>();

            config.SetRequiredNode<TechNodeMetalArmor>();
        }
    }
}