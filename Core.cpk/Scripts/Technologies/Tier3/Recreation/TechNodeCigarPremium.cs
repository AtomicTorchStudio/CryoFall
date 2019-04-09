namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Recreation
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeCigarPremium : TechNode<TechGroupRecreation>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeCigarPremium>();

            config.SetRequiredNode<TechNodeCigarNormal>();
        }
    }
}