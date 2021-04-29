namespace AtomicTorch.CBND.CoreMod.ItemContainers.Turrets
{
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.Items.Weapons.Turret;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ContainerTurretLightAmmo : BaseItemsContainerTurretAmmo
    {
        private IProtoItemWeapon protoWeapon;

        public override IProtoItemWeapon ProtoWeapon => this.protoWeapon;

        public override byte SlotsCount => 4;

        protected override void PrepareProto()
        {
            base.PrepareProto();
            this.protoWeapon = Api.GetProtoEntity<ItemWeaponTurretLight>();
        }
    }
}