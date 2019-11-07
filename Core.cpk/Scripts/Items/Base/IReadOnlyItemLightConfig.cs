namespace AtomicTorch.CBND.CoreMod.Items
{
    using System.Windows.Media;
    using AtomicTorch.GameEngine.Common.Primitives;

    public interface IReadOnlyItemLightConfig
    {
        Color Color { get; }

        bool IsLightEnabled { get; }

        Size2F? LogicalSize { get; }

        Vector2D ScreenOffset { get; }

        Size2F Size { get; }

        Vector2D WorldOffset { get; }
    }
}