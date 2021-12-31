namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Turret
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class ItemWeaponTurretHeavy : ProtoItemWeaponTurretWithAmmo
    {
        public override double CharacterAnimationAimingRecoilDuration => 1 / 5.0;

        public override double CharacterAnimationAimingRecoilPower => 0.667;

        public override double DamageMultiplier => 0.8; // slightly lower than default

        public override double FireInterval => 1 / 2.6667; // one-third of the heavy autocannon fire rate

        public override double RangeMultiplier => 0.8; // 8 tiles as ammo has range of 10 tiles

        public override double SpecialEffectProbability => 0.1;

        public override double SharedUpdateAndGetFirePatternCurrentSpreadAngleDeg(WeaponState state)
        {
            // angle variation within 15 degrees
            return 15 * (RandomHelper.NextDouble() - 0.5);
        }

        protected override void PrepareMuzzleFlashDescription(MuzzleFlashDescription description)
        {
            description.Set(MuzzleFlashPresets.ModernRifle)
                       .Set(textureScreenOffset: (85, -17), textureScale: 1.5);
        }

        protected override void PrepareProtoWeaponRanged(
            out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
            ref DamageDescription overrideDamageDescription)
        {
            compatibleAmmoProtos = GetAmmoOfType<IAmmoCaliber300>();
        }

        protected override ReadOnlySoundPreset<WeaponSound> PrepareSoundPresetWeapon()
        {
            return WeaponsSoundPresets.WeaponVehicleAutocannonHeavy;
        }

        protected override void ServerOnSpecialEffect(ICharacter damagedCharacter, double damage)
        {
            ServerWeaponSpecialEffectsHelper.OnFirearmHit(damagedCharacter, damage);
        }
    }
}