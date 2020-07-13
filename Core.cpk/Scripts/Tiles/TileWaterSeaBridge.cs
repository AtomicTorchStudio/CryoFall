namespace AtomicTorch.CBND.CoreMod.Tiles
{
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class TileWaterSeaBridge : TileWaterSea
    {
        public override double FishingKnowledgeLevelIncrease => 10;

        public override bool IsFishingAllowed => false;

        [NotLocalizable]
        public override string Name => "Sea bridge";

        public override string WorldMapTexturePath
            => "Map/Roads.png";

        public override void CreatePhysics(Tile tile, IPhysicsBody physicsBody)
        {
            // it's a bridge, no tile colliders
        }
    }
}