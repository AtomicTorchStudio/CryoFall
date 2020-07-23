namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Vehicle
{
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Weapons;

    public class ItemVehicleCannonEnergy : ProtoItemVehicleEnergyWeapon
    {
        public override double CharacterAnimationAimingRecoilDuration => 0.45;

        public override double CharacterAnimationAimingRecoilPower => 0.5;

        public override double CharacterAnimationAimingRecoilPowerAddCoef => 1;

        public override double DamageMultiplier => 1;

        public override string Description =>
            "Heavy cannon that uses coherent charges of accelerated particles to damage the target.";

        public override uint DurabilityMax => 2500;

        public override ushort EnergyUsePerShot => 750;

        public override double FireInterval => 1 / 2.5;

        public override string Name => "Energy cannon";

        public override double ReadyDelayDuration => 0;

        public override double SpecialEffectProbability => 0.1;

        public override string WeaponAttachmentName => "TurretLeft";

        public override VehicleWeaponHardpoint WeaponHardpoint => VehicleWeaponHardpoint.Large;

        protected override ProtoSkillWeapons WeaponSkill => GetSkill<SkillVehicles>();

        protected override WeaponFireScatterPreset PrepareFireScatterPreset()
        {
            return new WeaponFireScatterPreset(
                new[] { -6.0, -4.5, -3.0, -1.5, 0.0, 1.5, 3.0, 4.5, 6.0 });
        }

        protected override WeaponFireTracePreset PrepareFireTracePreset()
        {
            return WeaponFireTracePresets.Plasma;
        }

        protected override void PrepareMuzzleFlashDescription(MuzzleFlashDescription description)
        {
            description.Set(MuzzleFlashPresets.EnergyPlasmaWeapon)
                       .Set(textureScreenOffset: (15, 0), textureScale: 1.5);
        }

        protected override void PrepareProtoWeaponRangedEnergy(ref DamageDescription damageDescription)
        {
            damageDescription = new DamageDescription(
                damageValue: 25, // this may seem high, but because of the spread not all projectiles hit the target
                armorPiercingCoef: 0.3,
                finalDamageMultiplier: 2.0,
                // the range is the same as for the autocannons that are using 10 mm or .300 cal ammo
                rangeMax: 10,
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