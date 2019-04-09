namespace AtomicTorch.CBND.CoreMod.Technologies.Tier2.Construction2
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Floors;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Walls;

    public class TechNodeStoneConstructions : TechNode<TechGroupConstruction2>
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