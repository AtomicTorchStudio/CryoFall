namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Construction4
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Floors;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Walls;

    public class TechNodeSteelConstructions : TechNode<TechGroupConstruction4>
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