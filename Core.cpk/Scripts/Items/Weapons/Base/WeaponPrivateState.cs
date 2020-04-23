namespace AtomicTorch.CBND.CoreMod.Items.Weapons
{
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class WeaponPrivateState : ItemWithDurabilityPrivateState
    {
        [SyncToClient(isSendChanges: false)]
        public ushort AmmoCount { get; private set; }

        [SyncToClient(isSendChanges: false)]
        public IProtoItemAmmo CurrentProtoItemAmmo { get; set; }

        public void SetAmmoCount(ushort ammoCount)
        {
            this.AmmoCount = ammoCount;

            var item = (IItem)this.GameObject;
            ((IProtoItemWeapon)item.ProtoGameObject)
                .SharedOnWeaponAmmoChanged(item, ammoCount);
        }
    }
}