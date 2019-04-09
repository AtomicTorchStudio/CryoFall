namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Defense4
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeAssaultArmor : TechNode<TechGroupDefense4>
    {
        public override string Name => "Assault armor";

        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeAssaultJacket>()
                  .AddRecipe<RecipeAssaultPants>();

            config.SetRequiredNode<TechNodeApartSuit>();
        }
    }
}