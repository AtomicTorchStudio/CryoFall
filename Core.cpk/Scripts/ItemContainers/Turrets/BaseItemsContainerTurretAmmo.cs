namespace AtomicTorch.CBND.CoreMod.ItemContainers.Turrets
{
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public abstract class BaseItemsContainerTurretAmmo : ProtoItemsContainer
    {
        public abstract IProtoItemWeapon ProtoWeapon { get; }

        public abstract byte SlotsCount { get; }

        public override bool CanAddItem(CanAddItemContext context)
        {
            return context.Item.ProtoGameObject is IProtoItemAmmo protoItemAmmo
                   && this.ProtoWeapon.CompatibleAmmoProtos.Contains(protoItemAmmo);
        }

        public override bool CanRemoveItem(CanRemoveItemContext context)
        {
            return true;
        }
    }
}