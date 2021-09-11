namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemGrenadeLauncherMulti : ProtoItemWeaponGrenadeLauncher
    {
        public override ushort AmmoCapacity => 5;

        public override double AmmoReloadDuration => 6;

        public override double CharacterAnimationAimingRecoilDuration => 0.6;

        public override double CharacterAnimationAimingRecoilPower => 1.2;

        public override string Description =>
            "Revolver-type grenade launcher. Ideal for assault missions or when fighting against heavily armored targets.";

        public override uint DurabilityMax => 250;

        public override double FireInterval => 1; // 1 second

        public override string Name => "Multiple grenade launcher";

        public override double ReadyDelayDuration => 3;

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