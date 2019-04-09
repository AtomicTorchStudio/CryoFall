namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Construction3
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Fridges;

    public class TechNodeFridgeLarge : TechNode<TechGroupConstruction3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectFridgeLarge>();

            config.SetRequiredNode<TechNodeMetalBarrel>();
        }
    }
}