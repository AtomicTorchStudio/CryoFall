namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Defense4
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeSuperHeavyArmor : TechNode<TechGroupDefense4>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeSuperHeavyArmor>();

            config.SetRequiredNode<TechNodePragmiumArmor>();
        }
    }
}