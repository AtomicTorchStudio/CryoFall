namespace AtomicTorch.CBND.CoreMod.Items
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.World;

    // TODO: actually this is a pretty annoying hack and we would like to avoid this completely
    public interface IProtoWorldObjectWithSoundPresets : IProtoWorldObject
    {
        ObjectSoundMaterial ObjectSoundMaterial { get; }

        ReadOnlySoundPreset<ObjectSound> SoundPresetObject { get; }
    }
}