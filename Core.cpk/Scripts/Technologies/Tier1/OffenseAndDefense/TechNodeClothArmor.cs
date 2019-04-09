namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.OffenseAndDefense
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeClothArmor : TechNode<TechGroupOffenseAndDefense>
    {
        public override string Name => "Cloth armor";

        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeClothShirt>()
                  .AddRecipe<RecipeClothHat>()
                  .AddRecipe<RecipeClothPants>();

            config.SetRequiredNode<TechNodeThread>();
        }
    }
}