namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Construction4
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Floors;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Walls;

    public class TechNodeConcreteConstructions : TechNode<TechGroupConstruction4>
    {
        public override string Name => "Concrete constructions";

        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectWallConcrete>()
                  .AddStructure<ObjectFloorConcrete>();

            config.SetRequiredNode<TechNodeLandClaimT4>();
        }
    }
}