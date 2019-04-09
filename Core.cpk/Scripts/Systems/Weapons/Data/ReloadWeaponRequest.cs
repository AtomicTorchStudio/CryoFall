namespace AtomicTorch.CBND.CoreMod.Systems.Weapons
{
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    public struct ReloadWeaponRequest : IRemoteCallParameter
    {
        public ReloadWeaponRequest(
            IItem item,
            IProtoItemAmmo protoItemAmmo)
        {
            this.ProtoItemAmmo = protoItemAmmo;
            this.Item = item;
        }

        public IItem Item { get; }

        public IProtoItemAmmo ProtoItemAmmo { get; }
    }
}