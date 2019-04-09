namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Defense3
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeMilitaryHeadgear : TechNode<TechGroupDefense3>
    {
        public override string Name => "Military headgear";

        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeMilitaryHelmet>();

            config.SetRequiredNode<TechNodeMilitaryArmor>();
        }
    }
}