namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Defense
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeLeatherHeadgear : TechNode<TechGroupDefenseT2>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeLeatherHelmet>();

            config.SetRequiredNode<TechNodeLeatherArmor>();
        }
    }
}