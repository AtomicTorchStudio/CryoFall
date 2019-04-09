namespace AtomicTorch.CBND.CoreMod.Items.Equipment
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Resources;

    /// <summary>
    /// Item prototype for full body equipment.
    /// </summary>
    public abstract class ProtoItemEquipmentFullBody
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoItemEquipment
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoItemEquipmentFullBody
        where TPrivateState : BasePrivateState, IItemWithDurabilityPrivateState, new()
        where TPublicState : BasePublicState, new()
        where TClientState : BaseClientState, new()
    {
        public sealed override EquipmentType EquipmentType => EquipmentType.FullBody;

        public abstract bool IsHairVisible { get; }

        public override bool RequireEquipmentTextures => true;

        public ReadOnlySoundPreset<CharacterSound> SoundPresetCharacterOverride { get; private set; }

        public ReadOnlySoundPreset<GroundSoundMaterial> SoundPresetFootstepsOverride { get; private set; }

        /// <inheritdoc />
        public ReadOnlySoundPreset<CharacterSound> SoundPresetMovementOverride { get; private set; }

        protected override double DefenseMultiplier { get; } = DefaultDefenseMultipliers.FullBody;

        public virtual void ClientGetHeadSlotSprites(
            IItem item,
            bool isMale,
            SkeletonResource skeletonResource,
            bool isFrontFace,
            out string spriteFront,
            out string spriteBehind)
        {
            this.VerifyGameObject(item);
            var slotAttachments = isMale
                                      ? this.SlotAttachmentsMale
                                      : this.SlotAttachmentsFemale;

            ProtoItemEquipmentHeadHelper.ClientFindDefaultHeadSprites(
                slotAttachments,
                skeletonResource,
                isFrontFace,
                out spriteFront,
                out spriteBehind);
        }

        protected sealed override void PrepareProtoItemEquipment()
        {
            base.PrepareProtoItemEquipment();
            this.SoundPresetMovementOverride = this.PrepareSoundPresetMovementOverride();
            this.SoundPresetCharacterOverride = this.PrepareSoundPresetCharacterOverride();
            this.SoundPresetFootstepsOverride = this.PrepareSoundPresetFootstepsOverride();
            this.PrepareProtoItemEquipmentFullBody();
        }

        protected virtual void PrepareProtoItemEquipmentFullBody()
        {
        }

        protected virtual ReadOnlySoundPreset<CharacterSound> PrepareSoundPresetCharacterOverride()
        {
            return null;
        }

        protected virtual ReadOnlySoundPreset<GroundSoundMaterial> PrepareSoundPresetFootstepsOverride()
        {
            return null;
        }

        protected virtual ReadOnlySoundPreset<CharacterSound> PrepareSoundPresetMovementOverride()
        {
            return null;
        }

        protected override byte[] SharedGetCompatibleContainerSlotsIds()
        {
            // special case for full body armor, the item itself goes to Chest slot only
            // allow placing devices in any of these slots
            // (extra check will be performed to ensure only this armor equipped in any of these slots)
            return new[]
            {
                (byte)EquipmentType.Chest,
            };
        }
    }

    /// <summary>
    /// Item prototype for chest equipment.
    /// </summary>
    public abstract class ProtoItemEquipmentFullBody
        : ProtoItemEquipmentFullBody
            <ItemWithDurabilityPrivateState,
                EmptyPublicState,
                EmptyClientState>
    {
    }
}