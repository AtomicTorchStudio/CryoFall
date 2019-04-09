namespace AtomicTorch.CBND.CoreMod.Items.Weapons
{
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class WeaponPrivateState : ItemWithDurabilityPrivateState
    {
        [SyncToClient(isSendChanges: false)]
        public ushort AmmoCount { get; set; }

        [SyncToClient(isSendChanges: false)]
        public IProtoItemAmmo CurrentProtoItemAmmo { get; set; }
    }
}