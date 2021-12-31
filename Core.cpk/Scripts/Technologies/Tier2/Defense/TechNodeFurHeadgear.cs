namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Defense
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeFurHeadgear : TechNode<TechGroupDefenseT2>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeFurHelmet>();

            config.SetRequiredNode<TechNodeFurArmor>();
        }
    }
}