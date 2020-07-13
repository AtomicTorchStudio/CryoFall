namespace AtomicTorch.CBND.CoreMod.Items.Tools
{
    using AtomicTorch.GameEngine.Common.Primitives;

    public interface IProtoItemToolFishing
        : IProtoItemTool, IProtoItemWithHotbarOverlay, IProtoItemWithCharacterAppearance
    {
        Vector2F FishingLineStartScreenOffset { get; }

        double FishingSpeedMultiplier { get; }
    }
}