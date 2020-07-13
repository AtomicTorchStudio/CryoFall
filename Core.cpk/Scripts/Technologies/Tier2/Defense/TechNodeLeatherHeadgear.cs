namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Defense
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeLeatherHeadgear : TechNode<TechGroupDefenseT2>
    {
        public override string Name => "Leather headgear";

        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeLeatherHelmetCowboy>()
                  .AddRecipe<RecipeLeatherHelmetPilot>()
                  .AddRecipe<RecipeLeatherHelmetTricorne>();

            config.SetRequiredNode<TechNodeLeatherArmor>();
        }
    }
}