namespace AtomicTorch.CBND.CoreMod.Items
{
    using System.Windows.Media;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ItemLightConfig : IReadOnlyItemLightConfig
    {
        public Color Color { get; set; }

        public bool IsLightEnabled { get; set; } = true;

        public Size2F? LogicalSize { get; set; }

        public Vector2D ScreenOffset { get; set; }

        public Size2F Size { get; set; }

        public Vector2D WorldOffset { get; set; }

        public IReadOnlyItemLightConfig ToReadOnly()
        {
            return this;
        }
    }
}