namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.Defense
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeBoneArmor : TechNode<TechGroupDefenseT1>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeBoneArmor>()
                  .AddRecipe<RecipeBoneHelmet>();

            config.SetRequiredNode<TechNodeWoodArmor>();
        }
    }
}