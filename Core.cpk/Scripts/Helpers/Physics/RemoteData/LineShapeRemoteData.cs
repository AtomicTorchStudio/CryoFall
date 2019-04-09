namespace AtomicTorch.CBND.CoreMod.Helpers.Physics
{
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class LineShapeRemoteData : BasePhysicsShapeRemoteData
    {
        public LineShapeRemoteData(LineShape shape) : base(shape)
        {
            this.BasePosition = shape.BasePosition;
            this.Direction = shape.Direction;
        }

        public Vector2D BasePosition { get; }

        public Vector2D Direction { get; }
    }
}