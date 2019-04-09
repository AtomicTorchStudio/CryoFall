namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.OffenseAndDefense
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;

    public class TechNodeBlackpowder : TechNode<TechGroupOffenseAndDefense>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeBlackpowder>()
                  .AddStructure<ObjectWeaponWorkbench>();
        }
    }
}