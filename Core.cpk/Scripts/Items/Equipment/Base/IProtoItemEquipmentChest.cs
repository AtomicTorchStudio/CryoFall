namespace AtomicTorch.CBND.CoreMod.Items.Equipment
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public interface IProtoItemEquipmentChest : IProtoItemEquipment
    {
        /// <summary>
        /// Sound preset for character sounds played at "chest" on various events.
        /// It's supposed to be used to play various sounds such as clanking armor in moving loop and other events.
        /// </summary>
        ReadOnlySoundPreset<CharacterSound> SoundPresetMovementOverride { get; }
    }
}