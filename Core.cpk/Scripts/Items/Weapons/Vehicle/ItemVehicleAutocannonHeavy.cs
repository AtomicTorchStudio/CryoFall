namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Vehicle
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Weapons;

    public class ItemVehicleAutocannonHeavy : ProtoItemVehicleWeaponRanged
    {
        public override ushort AmmoCapacity => 100;

        public override double AmmoReloadDuration => 3;

        public override string CharacterAnimationAimingName => "WeaponAiming1Hand";

        public override double CharacterAnimationAimingRecoilDuration => 0.45;

        public override string CharacterAnimationAimingRecoilName => "WeaponShooting1Hand";

        public override double CharacterAnimationAimingRecoilPower => 0.5;

        public override double CharacterAnimationAimingRecoilPowerAddCoef
            => 1 / 2.5; // full recoil power will be gained on third shot

        public override double DamageMultiplier => 0.8; // slightly lower than default

        public override string Description =>
            "Heavy autocannon designed for hand-mounted hardpoints on mechanized battle armor. Uses high-caliber, anti-material ammo.";

        public override uint DurabilityMax => 2000;

        public override double FireInterval => 1 / 8.0; // 8 per second

        public override string Name => "Heavy autocannon";

        public override double SpecialEffectProbability => 0.1;

        public override string WeaponAttachmentName => "TurretLeft";

        public override VehicleWeaponHardpoint WeaponHardpoint => VehicleWeaponHardpoint.Normal;

        protected override ProtoSkillWeapons WeaponSkill => GetSkill<SkillVehicles>();

        protected override WeaponFirePatternPreset PrepareFirePatternPreset()
        {
            return new(
                initialSequence: new[] { 0.0, 1.0, 1.0 },
                cycledSequence: new[] { 2.0, 3.5, 3.0, 0.5, 4.0 });
        }

        protected override void PrepareMuzzleFlashDescription(MuzzleFlashDescription description)
        {
            description.Set(MuzzleFlashPresets.ModernSubmachinegun)
                       .Set(textureScreenOffset: (90, -65), textureScale: 1.5);
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