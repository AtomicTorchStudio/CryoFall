namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.Construction
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Floors;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Walls;

    public class TechNodeBasicBuilding : TechNode<TechGroupConstructionT1>
    {
        public override string Name => "Basic building";

        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectWallWood>()
                  .AddStructure<ObjectFloorWood>();

            config.SetRequiredNode<TechNodeLandClaimT1>();
        }
    }
}