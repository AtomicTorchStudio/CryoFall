namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemGrenadeLauncher : ProtoItemWeaponGrenadeLauncher
    {
        public override ushort AmmoCapacity => 1;

        public override double AmmoReloadDuration => 3;

        public override double CharacterAnimationAimingRecoilDuration => 0.6;

        public override double CharacterAnimationAimingRecoilPower => 1.2;

        public override string Description =>
            "Single grenade launcher fires specially designed large caliber rounds that explode upon impact, dealing massive damage.";

        public override uint DurabilityMax => 100;

        public override string Name => "Grenade launcher";

        public override double ReadyDelayDuration => 2;

        protected override void PrepareMuzzleFlashDescription(MuzzleFlashDescription description)
        {
            description.Set(MuzzleFlashPresets.PrimitiveRifle)
                       .Set(textureScreenOffset: (20, 11));
        }

        protected override void PrepareProtoGrenadeLauncher(out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos)
        {
            compatibleAmmoProtos = GetAmmoOfType<IAmmoGrenade>();
        }

        protected override ReadOnlySoundPreset<WeaponSound> PrepareSoundPresetWeapon()
        {
            return WeaponsSoundPresets.WeaponGrenadeLauncher;
        }
    }
}