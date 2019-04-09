namespace AtomicTorch.CBND.CoreMod.Items.Equipment
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.State;

    /// <summary>
    /// Item prototype for legs equipment.
    /// </summary>
    public abstract class ProtoItemEquipmentLegs
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoItemEquipment
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoItemEquipmentLegs
        where TPrivateState : BasePrivateState, IItemWithDurabilityPrivateState, new()
        where TPublicState : BasePublicState, new()
        where TClientState : BaseClientState, new()
    {
        public sealed override EquipmentType EquipmentType => EquipmentType.Legs;

        public override bool RequireEquipmentTextures => false;

        /// <inheritdoc />
        public ReadOnlySoundPreset<GroundSoundMaterial> SoundPresetFootstepsOverride { get; private set; }

        protected override double DefenseMultiplier { get; } = DefaultDefenseMultipliers.Legs;

        protected sealed override void PrepareProtoItemEquipment()
        {
            base.PrepareProtoItemEquipment();
            this.SoundPresetFootstepsOverride = this.PrepareSoundPresetFootstepsOverride();
            this.PrepareProtoItemEquipmentLegs();
        }

        protected virtual void PrepareProtoItemEquipmentLegs()
        {
        }

        protected virtual ReadOnlySoundPreset<GroundSoundMaterial> PrepareSoundPresetFootstepsOverride()
        {
            return null;
        }
    }

    /// <summary>
    /// Item prototype for legs equipment.
    /// </summary>
    public abstract class ProtoItemEquipmentLegs
        : ProtoItemEquipmentLegs
            <ItemWithDurabilityPrivateState,
                EmptyPublicState,
                EmptyClientState>
    {
    }
}