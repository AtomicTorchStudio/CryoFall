namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Weapons;

    public class ItemPlasmaPistol : ProtoItemWeaponRangedEnergy
    {
        public override string CharacterAnimationAimingName => "WeaponPistolAiming";

        public override string CharacterAnimationAimingRecoilName => "WeaponPistolShooting";

        public override string Description =>
            "Plasma pistol forms and projects concentrated plasma packets that burn the target upon impact, dealing heavy damage. High power consumption.";

        public override uint DurabilityMax => 600;

        public override double EnergyUsePerShot => 50;

        public override double FireInterval => 0.6;

        public override string Name => "Plasma pistol";

        public override double ReadyDelayDuration => 0.6;

        public override double SpecialEffectProbability => 0.25;

        protected override WeaponFireScatterPreset PrepareFireScatterPreset()
        {
            return new(
                new[] { -1.5, -0.5, 0.5, 1.5 });
        }

        protected override WeaponFireTracePreset PrepareFireTracePreset()
        {
            return WeaponFireTracePresets.Plasma;
        }

        protected override void PrepareMuzzleFlashDescription(MuzzleFlashDescription description)
        {
            description.Set(MuzzleFlashPresets.EnergyPlasmaWeapon)
                       .Set(textureScreenOffset: (-8, 0.2));
        }

        protected override void PrepareProtoWeaponRangedEnergy(
            ref DamageDescription damageDescription)
        {
            damageDescription = new DamageDescription(
                damageValue: 24, // this may seem high, but because of the spread not all projectiles hit the target
                armorPiercingCoef: 0.0,
                finalDamageMultiplier: 1.8,
                rangeMax: 8,
                damageDistribution: new DamageDistribution(DamageType.Heat, 1));
        }

        protected override ReadOnlySoundPreset<WeaponSound> PrepareSoundPresetWeapon()
        {
            return WeaponsSoundPresets.WeaponRangedPlasmaPistol;
        }

        protected override void ServerOnSpecialEffect(ICharacter damagedCharacter, double damage)
        {
            ServerWeaponSpecialEffectsHelper.OnPlasmaHit(damagedCharacter, damage);
        }
    }
}