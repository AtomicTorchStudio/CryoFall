namespace AtomicTorch.CBND.CoreMod.Items.Equipment
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.State;

    /// <summary>
    /// Item prototype for chest equipment.
    /// </summary>
    public abstract class ProtoItemEquipmentChest
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoItemEquipment
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoItemEquipmentChest
        where TPrivateState : BasePrivateState, IItemWithDurabilityPrivateState, new()
        where TPublicState : BasePublicState, new()
        where TClientState : BaseClientState, new()
    {
        public sealed override EquipmentType EquipmentType => EquipmentType.Chest;

        public override bool RequireEquipmentTextures => true;

        public abstract ObjectMaterial Material { get; }

        /// <inheritdoc />
        public ReadOnlySoundPreset<CharacterSound> SoundPresetMovementOverride { get; private set; }

        protected override double DefenseMultiplier { get; } = DefaultDefenseMultipliers.Chest;

        protected sealed override void PrepareProtoItemEquipment()
        {
            base.PrepareProtoItemEquipment();
            this.SoundPresetMovementOverride = this.PrepareSoundPresetMovementOverride();
            this.PrepareProtoItemEquipmentChest();
        }

        protected virtual void PrepareProtoItemEquipmentChest()
        {
        }

        protected virtual ReadOnlySoundPreset<CharacterSound> PrepareSoundPresetMovementOverride()
        {
            return null;
        }
    }

    /// <summary>
    /// Item prototype for chest equipment.
    /// </summary>
    public abstract class ProtoItemEquipmentChest
        : ProtoItemEquipmentChest
            <ItemWithDurabilityPrivateState,
                EmptyPublicState,
                EmptyClientState>
    {
    }
}