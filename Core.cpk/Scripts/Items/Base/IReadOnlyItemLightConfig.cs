namespace AtomicTorch.CBND.CoreMod.Items
{
    using System.Windows.Media;
    using AtomicTorch.GameEngine.Common.Primitives;

    public interface IReadOnlyItemLightConfig
    {
        Color Color { get; }

        bool IsLightEnabled { get; set; }

        Vector2D ScreenOffset { get; }

        Size2F Size { get; }

        Size2F? LogicalSize { get; set; }
    }
}