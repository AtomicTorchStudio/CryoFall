namespace AtomicTorch.CBND.CoreMod.Tiles
{
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class TileWaterLakeBridge : TileWaterLake
    {
        [NotLocalizable]
        public override string Name => "Lake bridge";

        public override string WorldMapTexturePath
            => "Map/Roads.png";

        public override void CreatePhysics(Tile tile, IPhysicsBody physicsBody)
        {
            // it's a bridge, no tile colliders
        }
    }
}