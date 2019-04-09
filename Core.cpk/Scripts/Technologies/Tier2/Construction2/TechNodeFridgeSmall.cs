namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Construction2
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Fridges;

    public class TechNodeFridgeSmall : TechNode<TechGroupConstruction2>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectFridgeSmall>();

            config.SetRequiredNode<TechNodeIronCrate>();
        }
    }
}