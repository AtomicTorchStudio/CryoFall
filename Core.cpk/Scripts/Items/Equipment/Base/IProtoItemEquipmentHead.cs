namespace AtomicTorch.CBND.CoreMod.Items.Equipment
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Resources;

    public interface IProtoItemEquipmentHead : IProtoItemEquipment
    {
        bool IsHairVisible { get; }

        bool IsHeadVisible { get; }

        /// <summary>
        /// Sound preset for character sounds played at "head" on various events.
        /// It's supposed to be used to play breathing sounds when character is in idle/moving loop.
        /// </summary>
        ReadOnlySoundPreset<CharacterSound> SoundPresetCharacterOverride { get; }

        void ClientGetHeadSlotSprites(
            IItem item,
            bool isMale,
            SkeletonResource skeletonResource,
            bool isFrontFace,
            out string spriteFront,
            out string spriteBehind);
    }
}