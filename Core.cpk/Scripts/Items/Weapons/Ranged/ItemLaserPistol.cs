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
            "Laser pistol emits a strong pulse of high-energy light, mostly in infrared spectrum, that burns the target.";

        public override uint DurabilityMax => 800;

        public override double EnergyUsePerShot => 20;

        public override double FireInterval => 1 / 3.0; // 3 shots per second

        public override string Name => "Laser pistol";

        public override double ReadyDelayDuration => 0.6;

        public override double SpecialEffectProbability => 0.25;

        protected override WeaponFirePatternPreset PrepareFirePatternPreset()
        {
            return new WeaponFirePatternPreset(
                initialSequence: new[] { 0.0, 0.5, -0.5 },
                cycledSequence: new[] { 0.0, 0.5, -0.5, 0.5, -0.5 });
        }

        protected override WeaponFireTracePreset PrepareFireTracePreset()
        {
            return WeaponFireTracePresets.Laser;
        }

        protected override void PrepareMuzzleFlashDescription(MuzzleFlashDescription description)
        {
            description.Set(MuzzleFlashPresets.EnergyLaserWeapon)
                       .Set(textureScreenOffset: (-4, 1.5));
        }

        protected override void PrepareProtoWeaponRangedEnergy(
            ref DamageDescription damageDescription)
        {
            damageDescription = new DamageDescription(
                damageValue: 12,
                armorPiercingCoef: 0.45,
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