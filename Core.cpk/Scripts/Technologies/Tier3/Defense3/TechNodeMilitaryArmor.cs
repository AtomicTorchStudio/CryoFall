namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Defense3
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeMilitaryArmor : TechNode<TechGroupDefense3>
    {
        public override string Name => "Military armor";

        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeMilitaryJacket>()
                  .AddRecipe<RecipeMilitaryPants>();

            config.SetRequiredNode<TechNodeMetalHeadgear>();
        }
    }
}