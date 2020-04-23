namespace AtomicTorch.CBND.CoreMod.Items.Tools
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public interface IProtoItemToolToolbox : IProtoItemTool, IProtoItemWithCharacterAppearance
    {
        double ConstructionSpeedMultiplier { get; }

        ReadOnlySoundPreset<ObjectSound> ObjectInteractionSoundsPreset { get; }
    }
}