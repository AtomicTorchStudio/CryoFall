namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Defense4
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodePragmiumArmor : TechNode<TechGroupDefense4>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipePragmiumArmor>();

            config.SetRequiredNode<TechNodeApartSuit>();
        }
    }
}