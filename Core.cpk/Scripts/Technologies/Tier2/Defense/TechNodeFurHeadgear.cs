namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Defense
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeFurHeadgear : TechNode<TechGroupDefenseT2>
    {
        public override string Name => "Fur headgear";

        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeFurHelmetCanadian>()
                  .AddRecipe<RecipeFurHelmetCossak>()
                  .AddRecipe<RecipeFurHelmetUshanka>();

            config.SetRequiredNode<TechNodeFurArmor>();
        }
    }
}