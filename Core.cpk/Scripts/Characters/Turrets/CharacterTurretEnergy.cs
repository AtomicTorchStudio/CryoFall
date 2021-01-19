namespace AtomicTorch.CBND.CoreMod.Characters.Turrets
{
    using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.Items.Weapons.Turret;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Defenses;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class CharacterTurretEnergy : ProtoCharacterTurret
    {
        public override double BarrelRotationRate => 8;

        public override string Name => Api.GetProtoEntity<ObjectTurretEnergy>().Name;

        protected override void PrepareProtoCharacterTurret(
            out ProtoItemWeaponTurret protoItemWeaponTurret,
            out ProtoCharacterSkeleton skeleton,
            ref double scale)
        {
            protoItemWeaponTurret = GetProtoEntity<ItemWeaponTurretEnergy>();
            skeleton = GetProtoEntity<SkeletonTurretEnergy>();
        }
    }
}