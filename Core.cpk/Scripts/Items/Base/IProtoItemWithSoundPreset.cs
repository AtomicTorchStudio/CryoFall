namespace AtomicTorch.CBND.CoreMod.Items
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public interface IProtoItemWithSoundPreset : IProtoItem
    {
        ReadOnlySoundPreset<ItemSound> SoundPresetItem { get; }
    }
}