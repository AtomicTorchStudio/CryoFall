namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Weapons;

    public class ItemLaserRifle : ProtoItemWeaponRangedEnergy
    {
        public override double CharacterAnimationAimingRecoilDuration => 0.2;

        public override double CharacterAnimationAimingRecoilPower => 0.3;

        public override double CharacterAnimationAimingRecoilPowerAddCoef
            => 1 / 2.5; // full recoil power will be gained on third shot

        public override string Description =>
            "Laser rifle emits quick pulses of high-energy light, mostly in infrared spectrum, that burns the target. Offers much higher firing rate than laser pistol at a cost of higher energy consumption.";

        public override uint DurabilityMax => 1600;

        public override double EnergyUsePerShot => 20;

        public override double FireInterval => 0.15; // about 6.67 shots per second

        public override string Name => "Laser rifle";

        public override double SpecialEffectProbability => 0.25;

        protected override WeaponFirePatternPreset PrepareFirePatternPreset()
        {
            return new WeaponFirePatternPreset(
                initialSequence: new[] { 0.0, 0.5, -0.5 },
                cycledSequence: new[] { 1.5, 2.0, 1.0, 0.0, -1.5, -2.0, -1.0, 0.0 });
        }

        protected override WeaponFireTracePreset PrepareFireTracePreset()
        {
            return WeaponFireTracePresets.Laser;
        }

        protected override void PrepareMuzzleFlashDescription(MuzzleFlashDescription description)
        {
            description.Set(MuzzleFlashPresets.EnergyLaserWeapon)
                       .Set(textureScreenOffset: (16, 2));
        }

        protected override void PrepareProtoWeaponRangedEnergy(
            ref DamageDescription damageDescription)
        {
            damageDescription = new DamageDescription(
                damageValue: 9,
                armorPiercingCoef: 0.6,
                finalDamageMultiplier: 1.1,
                rangeMax: 10,
                damageDistribution: new DamageDistribution(DamageType.Heat, 1));
        }

        protected override ReadOnlySoundPreset<WeaponSound> PrepareSoundPresetWeapon()
        {
            return WeaponsSoundPresets.WeaponRangedLaser;
        }

        protected override void ServerOnSpecialEffect(ICharacter damagedCharacter, double damage)
        {
            ServerWeaponSpecialEffectsHelper.OnLaserHit(damagedCharacter, damage);
        }
    }
}