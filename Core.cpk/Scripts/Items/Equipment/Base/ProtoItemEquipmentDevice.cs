namespace AtomicTorch.CBND.CoreMod.Items.Equipment
{
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Resources;

    /// <summary>
    /// Item prototype for device equipment.
    /// </summary>
    public abstract class ProtoItemEquipmentDevice
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoItemEquipment
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoItemEquipmentDevice
        where TPrivateState : BasePrivateState, IItemWithDurabilityPrivateState, new()
        where TPublicState : BasePublicState, new()
        where TClientState : BaseClientState, new()
    {
        public const string NotificationCannotUseDirectly_Message =
            "All devices are used automatically when equipped in the device slot.";

        public const string NotificationCannotUseDirectly_Title = "Cannot use directly";

        protected ProtoItemEquipmentDevice()
        {
            this.Icon = new TextureResource("Items/Devices/" + this.GetType().Name);
        }

        public sealed override EquipmentType EquipmentType => EquipmentType.Device;

        public override ITextureResource Icon { get; }

        public override bool RequireEquipmentTextures => false;

        protected override double DefenseMultiplier { get; } = 0;

        public override void ServerOnCharacterDeath(IItem item, bool isEquipped, out bool shouldDrop)
        {
            shouldDrop = true;
        }

        public override void ServerOnItemDamaged(IItem item, double damageApplied)
        {
            // no durability degradation
        }

        protected sealed override bool ClientItemUseFinish(ClientItemData data)
        {
            NotificationSystem.ClientShowNotification(
                NotificationCannotUseDirectly_Title,
                NotificationCannotUseDirectly_Message,
                NotificationColor.Bad,
                this.Icon);
            return false;
        }

        protected sealed override void ClientItemUseStart(ClientItemData data)
        {
        }

        protected sealed override void PrepareDefense(DefenseDescription defense)
        {
            // no defenses
        }

        protected override byte[] SharedGetCompatibleContainerSlotsIds()
        {
            // allow placing devices in these slots
            return new byte[]
            {
                (byte)EquipmentType.Device,
                (byte)EquipmentType.Device + 1,
                (byte)EquipmentType.Device + 2,
                (byte)EquipmentType.Device + 3,
                (byte)EquipmentType.Device + 4
            };
        }
    }

    /// <summary>
    /// Item prototype for device equipment.
    /// </summary>
    public abstract class ProtoItemEquipmentDevice
        : ProtoItemEquipmentDevice
            <ItemWithDurabilityPrivateState,
                EmptyPublicState,
                EmptyClientState>
    {
    }
}