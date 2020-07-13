namespace AtomicTorch.CBND.CoreMod.Technologies.Tier5.Construction
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Floors;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Walls;

    public class TechNodeSteelConstructions : TechNode<TechGroupConstructionT5>
    {
        public override string Name => "Steel constructions";

        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectWallArmored>()
                  .AddStructure<ObjectFloorMetal>();

            config.SetRequiredNode<TechNodeLandClaimT5>();
        }
    }
}