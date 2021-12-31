namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Defense
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeMilitaryHeadgear : TechNode<TechGroupDefenseT3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeMilitaryHelmet>();

            config.SetRequiredNode<TechNodeMilitaryArmor>();
        }
    }
}