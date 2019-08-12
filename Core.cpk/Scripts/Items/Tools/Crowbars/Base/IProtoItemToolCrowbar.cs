namespace AtomicTorch.CBND.CoreMod.Items.Tools.Crowbars
{
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public interface IProtoItemToolCrowbar
        : IProtoItemWithCharacterAppearance,
          IProtoItemWithDurablity
    {
        double DeconstructionSpeedMultiplier { get; }

        ReadOnlySoundPreset<ObjectSound> ObjectInteractionSoundsPreset { get; }
    }
}