namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Construction
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc;

    public class TechNodeTinkerTable : TechNode<TechGroupConstructionT2>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectTinkerTable>()
                  .AddRecipe<RecipeDuctTape>();

            config.SetRequiredNode<TechNodeIronCrate>();
        }
    }
}