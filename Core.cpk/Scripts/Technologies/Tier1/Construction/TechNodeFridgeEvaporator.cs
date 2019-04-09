namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.Construction
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Fridges;

    public class TechNodeFridgeEvaporator : TechNode<TechGroupConstruction>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectFridgeEvaporator>();

            config.SetRequiredNode<TechNodeWaterCollector>();
        }
    }
}