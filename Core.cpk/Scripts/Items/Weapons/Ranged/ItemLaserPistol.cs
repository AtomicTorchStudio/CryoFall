namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Weapons;

    public class ItemLaserPistol : ProtoItemWeaponRangedEnergy
    {
        public override string CharacterAnimationAimingName => "WeaponPistolAiming";

        public override double CharacterAnimationAimingRecoilDuration => 0.167;

        public override string CharacterAnimationAimingRecoilName => "WeaponPistolShooting";

        public override double CharacterAnimationAimingRecoilPower => 0.45;

        public override string Description =>
            "Laser pistol emits quick pulses of high-energy light that burn the target.";

        public override uint DurabilityMax => 3000;

        public override double EnergyUsePerShot => 10;

        public override double FireInterval => 0.15;

        public override string Name => "Laser pistol";

        public override double SpecialEffectProbability => 0.25;

        protected override WeaponFirePatternPreset PrepareFirePatternPreset()
        {
            return new(
                initialSequence: new[] { 0.0, 0.5, 0.5 },
                cycledSequence: new[] { 1.5, 2.0, 1.0, 0.0 });
        }

        protected override WeaponFireTracePreset PrepareFireTracePreset()
        {
            return WeaponFireTracePresets.LaserBlue;
        }

        protected override void PrepareMuzzleFlashDescription(MuzzleFlashDescription description)
        {
            description.Set(MuzzleFlashPresets.EnergyLaserWeaponBlue)
                       .Set(textureScreenOffset: (-8, 6));
        }

        protected override void PrepareProtoWeaponRangedEnergy(
            ref DamageDescription damageDescription)
        {
            damageDescription = new DamageDescription(
                damageValue: 9,
                armorPiercingCoef: 0.5,
                finalDamageMultiplier: 1.1,
                rangeMax: 9,
                damageDistribution: new DamageDistribution(DamageType.Heat, 1));
        }

        protected override ReadOnlySoundPreset<WeaponSound> PrepareSoundPresetWeapon()
        {
            return WeaponsSoundPresets.WeaponRangedLaserPistol;
        }

        protected override void ServerOnSpecialEffect(ICharacter damagedCharacter, double damage)
        {
            ServerWeaponSpecialEffectsHelper.OnLaserHit(damagedCharacter, damage);
        }
    }
}