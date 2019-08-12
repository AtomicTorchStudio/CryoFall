namespace AtomicTorch.CBND.CoreMod.Items.Tools.Toolboxes
{
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public interface IProtoItemToolToolbox : IProtoItemTool, IProtoItemWithCharacterAppearance
    {
        double ConstructionSpeedMultiplier { get; }

        ReadOnlySoundPreset<ObjectSound> ObjectInteractionSoundsPreset { get; }
    }
}