namespace AtomicTorch.CBND.CoreMod.Items.Equipment
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public interface IProtoItemEquipmentLegs : IProtoItemEquipment
    {
        /// <summary>
        /// Sound preset for character movement sounds on various ground materials.
        /// If character has this item equipped and this property is not null, it will be used as the sounds provider.
        /// </summary>
        ReadOnlySoundPreset<GroundSoundMaterial> SoundPresetFootstepsOverride { get; }
    }
}