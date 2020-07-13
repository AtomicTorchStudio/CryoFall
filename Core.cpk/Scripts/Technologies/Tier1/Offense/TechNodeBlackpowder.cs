namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.Offense
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;

    public class TechNodeBlackpowder : TechNode<TechGroupOffenseT1>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeBlackpowder>()
                  .AddStructure<ObjectWeaponWorkbench>();
        }
    }
}