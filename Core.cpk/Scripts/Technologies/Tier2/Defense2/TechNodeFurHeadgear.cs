namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Defense2
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeFurHeadgear : TechNode<TechGroupDefense2>
    {
        public override string Name => "Fur headgear";

        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeFurHatCanadian>()
                  .AddRecipe<RecipeFurHatCossak>()
                  .AddRecipe<RecipeFurHatUshanka>();

            config.SetRequiredNode<TechNodeFurArmor>();
        }
    }
}