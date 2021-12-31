namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Turret
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class ItemWeaponTurretEnergy : ProtoItemWeaponTurretEnergy
    {
        public override double CharacterAnimationAimingRecoilDuration => 0.45;

        public override double CharacterAnimationAimingRecoilPower => 0.5;

        public override double EnergyUsagePerShot => 120;

        public override double FireInterval => 1 / 2.5; // 5 per 2 seconds

        public override double SpecialEffectProbability => 0.1;

        public override double SharedUpdateAndGetFirePatternCurrentSpreadAngleDeg(WeaponState state)
        {
            // angle variation within 15 degrees
            return 15 * (RandomHelper.NextDouble() - 0.5);
        }

        protected override WeaponFireTracePreset PrepareFireTracePreset()
        {
            return WeaponFireTracePresets.Plasma;
        }

        protected override void PrepareMuzzleFlashDescription(MuzzleFlashDescription description)
        {
            description.Set(MuzzleFlashPresets.EnergyPlasmaWeapon)
                       .Set(textureScreenOffset: (85, -17), textureScale: 1.5);
        }

        protected override void PrepareProtoWeaponRanged(
            out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
            ref DamageDescription overrideDamageDescription)
        {
            compatibleAmmoProtos = null;

            overrideDamageDescription = new DamageDescription(
                damageValue: 25,
                armorPiercingCoef: 0.3,
                finalDamageMultiplier: 1.0,
                rangeMax: 8,
                damageDistribution: new DamageDistribution(DamageType.Heat, 1));
        }

        protected override ReadOnlySoundPreset<ObjectMaterial> PrepareSoundPresetHit()
        {
            return MaterialHitsSoundPresets.RangedEnergy;
        }

        protected override ReadOnlySoundPreset<WeaponSound> PrepareSoundPresetWeapon()
        {
            return WeaponsSoundPresets.WeaponVehicleEnergyCannon;
        }

        protected override void ServerOnSpecialEffect(ICharacter damagedCharacter, double damage)
        {
            ServerWeaponSpecialEffectsHelper.OnPlasmaHit(damagedCharacter, damage);
        }
    }
}