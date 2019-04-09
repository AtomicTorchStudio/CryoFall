namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.Construction
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Lights;

    public class TechNodeFloorLampOil : TechNode<TechGroupConstruction>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectLightFloorLampOil>()
                  .AddRecipe<RecipeCampFuelFromFat>();

            config.SetRequiredNode<TechNodeWaterCollector>();
        }
    }
}