namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Defense
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeMetalHeadgear : TechNode<TechGroupDefenseT3>
    {
        public override string Name => "Metal headgear";

        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeMetalHelmetClosed>()
                  .AddRecipe<RecipeMetalHelmetSkull>();

            config.SetRequiredNode<TechNodeMetalArmor>();
        }
    }
}