namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Defense2
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeFurArmor : TechNode<TechGroupDefense2>
    {
        public override string Name => "Fur armor";

        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeFurCoat>()
                  .AddRecipe<RecipeFurPants>();

            config.SetRequiredNode<TechNodeQuiltedHeadgear>();
        }
    }
}