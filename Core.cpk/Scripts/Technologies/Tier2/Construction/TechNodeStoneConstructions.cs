namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Construction
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Floors;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Walls;

    public class TechNodeStoneConstructions : TechNode<TechGroupConstructionT2>
    {
        public override string Name => "Stone constructions";

        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectWallStone>()
                  .AddStructure<ObjectFloorStone>();

            config.SetRequiredNode<TechNodeLandClaimT2>();
        }
    }
}