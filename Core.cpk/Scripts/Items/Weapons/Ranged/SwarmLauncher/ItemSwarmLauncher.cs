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

    public class ItemSwarmLauncher : ProtoItemWeaponRanged
    {
        public override ushort AmmoCapacity => 100;

        public override double AmmoReloadDuration => 4;

        public override double CharacterAnimationAimingRecoilDuration => 0.4;

        public override double CharacterAnimationAimingRecoilPower => 0.45;

        public override string Description =>
            "Exotic weapon that launches small insect-like creatures at enemies instead of bullets.";

        public override uint DurabilityMax => 1200;

        public override double FireInterval => 1 / 4.0; // 4 per second

        public override string Name => "Swarm launcher";

        public override double SpecialEffectProbability => 1.0;

        protected override ProtoSkillWeapons WeaponSkill => GetSkill<SkillWeaponsExotic>();

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
            description.Set(MuzzleFlashPresets.Poison)
                       .Set(textureScreenOffset: (236, 72));
        }

        protected override void PrepareProtoWeaponRanged(
            out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
            ref DamageDescription overrideDamageDescription)
        {
            compatibleAmmoProtos = GetAmmoOfType<IAmmoKeinite>();

            overrideDamageDescription = new DamageDescription(
                damageValue: 26,
                armorPiercingCoef: 0.2,
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
            damagedCharacter.ServerAddStatusEffect<StatusEffectPain>(intensity: 0.1);

            // add nausea if its current intensity is below 10%
            if (!damagedCharacter.SharedHasStatusEffect<StatusEffectNausea>(minIntensity: 0.1 - 0.025))
            {
                damagedCharacter.ServerAddStatusEffect<StatusEffectNausea>(intensity: 0.025);
            }
        }
    }
}