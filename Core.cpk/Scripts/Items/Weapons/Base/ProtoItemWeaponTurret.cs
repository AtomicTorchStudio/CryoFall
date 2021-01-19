namespace AtomicTorch.CBND.CoreMod.Items.Weapons
{
    using AtomicTorch.CBND.CoreMod.Items.Weapons.MobWeapons;

    public abstract class ProtoItemWeaponTurret : ProtoItemMobWeaponRanged
    {
        public override string CharacterAnimationAimingName => "animation";

        public override string CharacterAnimationAimingRecoilName => "recoil";

        public sealed override double ReadyDelayDuration => 0;

        public override string WeaponAttachmentName => "TurretBarrel1";
    }
}