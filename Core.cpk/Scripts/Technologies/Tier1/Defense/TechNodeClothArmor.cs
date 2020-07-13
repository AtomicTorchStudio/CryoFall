namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.Defense
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeClothArmor : TechNode<TechGroupDefenseT1>
    {
        public override string Name => "Cloth armor";

        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeClothShirt>()
                  .AddRecipe<RecipeClothHat>();

            config.SetRequiredNode<TechNodeThread>();
        }
    }
}