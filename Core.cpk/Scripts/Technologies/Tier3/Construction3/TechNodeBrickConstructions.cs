namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Construction3
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Floors;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Walls;

    public class TechNodeBrickConstructions : TechNode<TechGroupConstruction3>
    {
        public override string Name => "Brick constructions";

        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectWallBrick>()
                  .AddStructure<ObjectFloorBricks>();

            config.SetRequiredNode<TechNodeBricks>();
        }
    }
}