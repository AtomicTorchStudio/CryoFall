namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Weapons;

    public class ItemPlasmaRifle : ProtoItemWeaponRangedEnergy
    {
        public override double CharacterAnimationAimingRecoilDuration => 0.45;

        public override double CharacterAnimationAimingRecoilPower => 1.3;

        public override double CharacterAnimationAimingRecoilPowerAddCoef
            => 1 / 2.5; // full recoil power will be gained on third shot

        public override string Description =>
            "Plasma rifle forms and projects large, concentrated plasma packets that severely burn the target upon impact, dealing massive damage. Very high power consumption.";

        public override ushort DurabilityMax => 400;

        public override double EnergyUsePerShot => 150;

        public override double FireInterval => 0.7;

        public override string Name => "Plasma rifle";

        public override double SpecialEffectProbability => 0.25;

        protected override void PrepareMuzzleFlashDescription(MuzzleFlashDescription description)
        {
            description.Set(MuzzleFlashPresets.EnergyPlasmaWeapon)
                       .Set(textureScreenOffset: (6, 2));
        }

        protected override void PrepareProtoWeaponRangedEnergy(
            ref DamageDescription damageDescription)
        {
            damageDescription = new DamageDescription(
                damageValue: 24,
                armorPiercingCoef: 0.0,
                finalDamageMultiplier: 2.2,
                rangeMax: 8,
                damageDistribution: new DamageDistribution(DamageType.Heat, 1));
        }

        protected override ReadOnlySoundPreset<WeaponSound> PrepareSoundPresetWeapon()
        {
            return WeaponsSoundPresets.WeaponRangedPlasma;
        }

        protected override void ServerOnSpecialEffect(ICharacter damagedCharacter, double damage)
        {
            ServerWeaponSpecialEffectsHelper.OnLaserHit(damagedCharacter, damage);
        }
    }
}