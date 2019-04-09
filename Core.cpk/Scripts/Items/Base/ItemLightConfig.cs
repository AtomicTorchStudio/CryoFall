namespace AtomicTorch.CBND.CoreMod.Items
{
    using System.Windows.Media;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ItemLightConfig : IReadOnlyItemLightConfig
    {
        public ItemLightConfig(Color color, double size, Vector2D? screenOffset = null)
        {
            this.Color = color;
            this.Size = size;
            this.ScreenOffset = screenOffset ?? Vector2D.Zero;
        }

        public ItemLightConfig()
        {
        }

        public Color Color { get; set; }

        public bool IsLightEnabled { get; set; } = true;

        public Vector2D ScreenOffset { get; set; }

        public double Size { get; set; }

        public IReadOnlyItemLightConfig ToReadOnly()
        {
            return this;
        }
    }
}