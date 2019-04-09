namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.Construction
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;

    public class TechNodeWaterCollector : TechNode<TechGroupConstruction>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectWaterCollector>();

            config.SetRequiredNode<TechNodeWoodCrate>();
        }
    }
}