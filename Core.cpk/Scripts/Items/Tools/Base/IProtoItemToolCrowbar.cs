namespace AtomicTorch.CBND.CoreMod.Items.Tools
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public interface IProtoItemToolCrowbar
        : IProtoItemWithCharacterAppearance,
          IProtoItemWithDurability
    {
        double DeconstructionSpeedMultiplier { get; }

        ReadOnlySoundPreset<ObjectSound> ObjectInteractionSoundsPreset { get; }
    }
}