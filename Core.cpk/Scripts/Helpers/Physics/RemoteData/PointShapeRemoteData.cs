namespace AtomicTorch.CBND.CoreMod.Helpers.Physics
{
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class PointShapeRemoteData : BasePhysicsShapeRemoteData
    {
        public PointShapeRemoteData(PointShape shape) : base(shape)
        {
            this.Point = shape.Point;
        }

        public Vector2D Point { get; }
    }
}