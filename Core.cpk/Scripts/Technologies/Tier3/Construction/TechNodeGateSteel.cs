namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Construction
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Doors;

    public class TechNodeGateSteel : TechNode<TechGroupConstructionT3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectGateSteelH>()
                  .AddStructure<ObjectGateSteelV>();

            config.SetRequiredNode<TechNodeBrickConstructions>();
        }
    }
}