namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Construction4
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Doors;

    public class TechNodeGateSteel : TechNode<TechGroupConstruction4>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectGateSteelH>()
                  .AddStructure<ObjectGateSteelV>();

            config.SetRequiredNode<TechNodeCistern>();
        }
    }
}