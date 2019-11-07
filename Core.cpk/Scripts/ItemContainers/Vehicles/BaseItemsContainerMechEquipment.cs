namespace AtomicTorch.CBND.CoreMod.ItemContainers.Vehicles
{
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public abstract class BaseItemsContainerMechEquipment : ProtoItemsContainer
    {
        public abstract byte AmmoSlotsCount { get; }

        public byte TotalSlotsCount => (byte)(this.WeaponSlotsCount + this.AmmoSlotsCount);

        public abstract byte WeaponSlotsCount { get; }

        public override bool CanAddItem(CanAddItemContext context)
        {
            if (!context.SlotId.HasValue)
            {
                return false;
            }

            var allowedSlotsIds = this.GetAllowedSlotsIds(context.Item.ProtoItem);
            if (allowedSlotsIds == null)
            {
                return false;
            }

            return allowedSlotsIds.Contains(context.SlotId.Value);
        }

        public override bool CanRemoveItem(CanRemoveItemContext context)
        {
            return true;
        }

        public override byte? FindSlotForItem(IItemsContainer container, IProtoItem protoItem)
        {
            var allowedSlotsIds = this.GetAllowedSlotsIds(protoItem);
            if (allowedSlotsIds == null)
            {
                return null;
            }

            if (allowedSlotsIds.Length == 1)
            {
                // return only one appropriate slot
                return allowedSlotsIds[0];
            }

            // this equipment type allows placing to multiple slots
            // find an empty one of them
            foreach (var allowedSlotsId in allowedSlotsIds)
            {
                if (!container.IsSlotOccupied(allowedSlotsId))
                {
                    // empty slot found
                    return allowedSlotsId;
                }
            }

            // no empty slots found, return first allowed slot id
            return allowedSlotsIds[0];
        }

        private byte[] GetAllowedSlotsIds(IProtoItem protoItem)
        {
            return protoItem switch
            {
                IProtoItemWeaponForMech _ => ByteSequence(0,                     this.WeaponSlotsCount),
                IProtoItemAmmo _          => ByteSequence(this.WeaponSlotsCount, this.TotalSlotsCount),
                _                         => null
            };

            static byte[] ByteSequence(byte fromInclusive, byte toExclusive)
            {
                var count = toExclusive - fromInclusive;
                var result = new byte[count];
                for (byte index = 0; index < count; index++)
                {
                    result[index] = (byte)(fromInclusive + index);
                }

                return result;
            }
        }
    }
}