namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Vehicle
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Weapons;

    public class ItemVehicleSwarmLauncher : ProtoItemVehicleWeaponRanged
    {
        public override ushort AmmoCapacity => 100;

        public override double AmmoReloadDuration => 4;

        public override string CharacterAnimationAimingName => "WeaponAiming1Hand";

        public override double CharacterAnimationAimingRecoilDuration => 0.45;

        public override string CharacterAnimationAimingRecoilName => "WeaponShooting1Hand";

        public override double CharacterAnimationAimingRecoilPower => 0.5;

        public override double CharacterAnimationAimingRecoilPowerAddCoef
            => 1 / 2.5; // full recoil power will be gained on third shot

        public override string Description =>
            "Launches large insect-like creatures which burrow into the target upon hit dealing additional damage.";

        public override uint DurabilityMax => 2000;

        public override double FireInterval => 1 / 5.0;

        public override string Name => "Heavy swarm launcher";

        public override double SpecialEffectProbability => 1.0;

        public override string WeaponAttachmentName => "TurretLeft";

        public override VehicleWeaponHardpoint WeaponHardpoint => VehicleWeaponHardpoint.Exotic;

        protected override ProtoSkillWeapons WeaponSkill => GetSkill<SkillWeaponsExotic>();

        public override ushort AmmoConsumptionPerShot => 2;

        protected override string GenerateIconPath()
        {
            return "Items/Weapons/Ranged/" + this.GetType().Name;
        }

        protected override WeaponFireScatterPreset PrepareFireScatterPreset()
        {
            return new(
                new[] { -3.0, -1.5, 0.0, 1.5, 3.0 });
        }

        protected override WeaponFirePatternPreset PrepareFirePatternPreset()
        {
            return new(
                initialSequence: new[] { 0.0, 1.0, 1.5 },
                cycledSequence: new[] { 2.0 });
        }

        protected override WeaponFireTracePreset PrepareFireTracePreset()
        {
            return WeaponFireTracePresets.ExoticWeaponPoison;
        }

        protected override void PrepareMuzzleFlashDescription(MuzzleFlashDescription description)
        {
            description.Set(MuzzleFlashPresets.None)
                       .Set(textureScreenOffset: (14, 9));
        }

        protected override void PrepareProtoWeaponRanged(
            out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
            ref DamageDescription overrideDamageDescription)
        {
            compatibleAmmoProtos = GetAmmoOfType<IAmmoKeinite>();

            overrideDamageDescription = new DamageDescription(
                damageValue: 30,
                armorPiercingCoef: 0.45,
                finalDamageMultiplier: 1.25,
                rangeMax: 10,
                new DamageDistribution(DamageType.Chemical, 1));
        }

        protected override ReadOnlySoundPreset<WeaponSound> PrepareSoundPresetWeapon()
        {
            return WeaponsSoundPresets.WeaponRangedExotic;
        }

        protected override void ServerOnSpecialEffect(ICharacter damagedCharacter, double damage)
        {
            damagedCharacter.ServerAddStatusEffect<StatusEffectPain>(intensity: 0.15);

            // add nausea if its current intensity is below 10%
            if (!damagedCharacter.SharedHasStatusEffect<StatusEffectNausea>(minIntensity: 0.1 - 0.025))
            {
                damagedCharacter.ServerAddStatusEffect<StatusEffectNausea>(intensity: 0.025);
            }
        }
    }
}