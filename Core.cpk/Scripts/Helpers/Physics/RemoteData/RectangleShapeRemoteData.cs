namespace AtomicTorch.CBND.CoreMod.Helpers.Physics
{
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class RectangleShapeRemoteData : BasePhysicsShapeRemoteData
    {
        public RectangleShapeRemoteData(RectangleShape shape) : base(shape)
        {
            this.Position = shape.Position;
            this.Size = shape.Size;
        }

        public Vector2D Position { get; }

        public Vector2D Size { get; }
    }
}