namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemGrenadeLauncher : ProtoItemWeaponGrenadeLauncher
    {
        public override ushort AmmoCapacity => 1;

        public override double AmmoReloadDuration => 2.5;

        public override double CharacterAnimationAimingRecoilDuration => 0.6;

        public override double CharacterAnimationAimingRecoilPower => 1.2;

        public override string Description =>
            "Single grenade launcher fires specially designed large caliber rounds that explode upon impact, dealing massive damage.";

        public override uint DurabilityMax => 200;

        public override double FireInterval => 0; // can fire as soon as reloaded

        public override string Name => "Grenade launcher";

        public override double ReadyDelayDuration => 1.5;

        protected override void PrepareMuzzleFlashDescription(MuzzleFlashDescription description)
        {
            description.Set(MuzzleFlashPresets.GrenadeLauncher)
                       .Set(textureScreenOffset: (20, 11));
        }

        protected override void PrepareProtoGrenadeLauncher(out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos)
        {
            compatibleAmmoProtos = GetAmmoOfType<IAmmoGrenadeForGrenadeLauncher>();
        }

        protected override ReadOnlySoundPreset<WeaponSound> PrepareSoundPresetWeapon()
        {
            return WeaponsSoundPresets.WeaponGrenadeLauncher;
        }
    }
}