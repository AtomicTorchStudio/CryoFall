namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Defense2
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeLeatherHeadgear : TechNode<TechGroupDefense2>
    {
        public override string Name => "Leather headgear";

        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeLeatherHatCowboy>()
                  .AddRecipe<RecipeLeatherHatPilot>()
                  .AddRecipe<RecipeLeatherHatTricorne>();

            config.SetRequiredNode<TechNodeLeatherArmor>();
        }
    }
}