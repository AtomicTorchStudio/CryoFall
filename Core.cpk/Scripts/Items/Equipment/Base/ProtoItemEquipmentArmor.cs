namespace AtomicTorch.CBND.CoreMod.Items.Equipment
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.State;

    /// <summary>
    /// Item prototype for chest equipment.
    /// </summary>
    public abstract class ProtoItemEquipmentArmor
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoItemEquipment
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoItemEquipmentArmor
        where TPrivateState : BasePrivateState, IItemWithDurabilityPrivateState, new()
        where TPublicState : BasePublicState, new()
        where TClientState : BaseClientState, new()
    {
        public sealed override EquipmentType EquipmentType => EquipmentType.Armor;

        public abstract ObjectMaterial Material { get; }

        public override bool RequireEquipmentTextures => true;

        public ReadOnlySoundPreset<GroundSoundMaterial> SoundPresetFootstepsOverride { get; private set; }

        /// <inheritdoc />
        public ReadOnlySoundPreset<CharacterSound> SoundPresetMovementOverride { get; private set; }

        protected override double DefenseMultiplier => DefaultDefenseMultipliers.Armor;

        protected sealed override void PrepareProtoItemEquipment()
        {
            base.PrepareProtoItemEquipment();
            this.SoundPresetMovementOverride = this.PrepareSoundPresetMovementOverride();
            this.SoundPresetFootstepsOverride = this.PrepareSoundPresetFootstepsOverride();
            this.PrepareProtoItemEquipmentChest();
        }

        protected virtual void PrepareProtoItemEquipmentChest()
        {
        }

        protected virtual ReadOnlySoundPreset<GroundSoundMaterial> PrepareSoundPresetFootstepsOverride()
        {
            return null;
        }

        protected virtual ReadOnlySoundPreset<CharacterSound> PrepareSoundPresetMovementOverride()
        {
            return null;
        }
    }

    /// <summary>
    /// Item prototype for chest equipment.
    /// </summary>
    public abstract class ProtoItemEquipmentArmor
        : ProtoItemEquipmentArmor
            <ItemWithDurabilityPrivateState,
                EmptyPublicState,
                EmptyClientState>
    {
    }
}