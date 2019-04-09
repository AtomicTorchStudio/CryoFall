namespace AtomicTorch.CBND.CoreMod.Helpers.Physics
{
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class LineSegmentShapeRemoteData : BasePhysicsShapeRemoteData
    {
        public LineSegmentShapeRemoteData(LineSegmentShape shape) : base(shape)
        {
            this.Point1 = shape.Point1;
            this.Point2 = shape.Point2;
        }

        public Vector2D Point1 { get; }

        public Vector2D Point2 { get; }
    }
}