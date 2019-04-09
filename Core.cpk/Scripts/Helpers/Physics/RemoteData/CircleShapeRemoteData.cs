namespace AtomicTorch.CBND.CoreMod.Helpers.Physics
{
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class CircleShapeRemoteData : BasePhysicsShapeRemoteData
    {
        public CircleShapeRemoteData(CircleShape shape) : base(shape)
        {
            this.Center = shape.Center;
            this.Radius = shape.Radius;
        }

        public Vector2D Center { get; }

        public double Radius { get; }
    }
}