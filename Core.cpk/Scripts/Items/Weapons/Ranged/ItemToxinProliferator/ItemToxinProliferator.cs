namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Weapons;

    public class ItemToxinProliferator : ProtoItemWeaponRanged
    {
        public override ushort AmmoCapacity => 100;

        public override double AmmoReloadDuration => 4;

        public override double CharacterAnimationAimingRecoilDuration => 0.33;

        public override double CharacterAnimationAimingRecoilPower => 0.15;

        public override string Description =>
            "Exotic weapon that uses liquified keinite to create and launch a potent toxin at enemies.";

        public override uint DurabilityMax => 800;

        public override double FireInterval => 0.5;

        public override string Name => "Toxin proliferator";

        public override double SpecialEffectProbability => 1.0;

        protected override ProtoSkillWeapons WeaponSkill => GetSkill<SkillWeaponsExotic>();

        protected override WeaponFirePatternPreset PrepareFirePatternPreset()
        {
            return new(
                initialSequence: new[] { 0.0, 0.2, 0.4 },
                cycledSequence: new[] { 0.5 });
        }

        protected override WeaponFireTracePreset PrepareFireTracePreset()
        {
            return WeaponFireTracePresets.ExoticWeaponPoisonBig;
        }

        protected override void PrepareMuzzleFlashDescription(MuzzleFlashDescription description)
        {
            description.Set(MuzzleFlashPresets.None)
                       .Set(textureScreenOffset: (261, 96));
        }

        protected override void PrepareProtoWeaponRanged(
            out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
            ref DamageDescription overrideDamageDescription)
        {
            compatibleAmmoProtos = GetAmmoOfType<IAmmoKeinite>();

            overrideDamageDescription = new DamageDescription(
                damageValue: 25,
                armorPiercingCoef: 0,
                finalDamageMultiplier: 1.5,
                rangeMax: 12,
                new DamageDistribution(DamageType.Chemical, 1));
        }

        protected override ReadOnlySoundPreset<WeaponSound> PrepareSoundPresetWeapon()
        {
            return WeaponsSoundPresets.WeaponRangedExotic;
        }

        protected override void ServerOnSpecialEffect(ICharacter damagedCharacter, double damage)
        {
            // guaranteed small toxin effect per hit
            // (please note the addition formula inside the Toxins effect class is NOT linear
            //  so subsequent hits increasing it on less than 20%)
            damagedCharacter.ServerAddStatusEffect<StatusEffectToxins>(intensity: 0.6);
        }
    }
}