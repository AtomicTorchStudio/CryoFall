namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Weapons;

    public class ItemRifle10mm : ProtoItemWeaponRanged
    {
        public override ushort AmmoCapacity => 8;

        public override double AmmoReloadDuration => 3;

        public override double CharacterAnimationAimingRecoilDuration => 0.4;

        public override double CharacterAnimationAimingRecoilPower => 1.1;

        public override double DamageMultiplier => 1.1;

        public override string Description => "Light automatic sniper rifle developed for 10mm ammo.";

        public override uint DurabilityMax => 250;

        public override double FireInterval => 0.6;

        public override string Name => "Light rifle";

        public override double RangeMultipier => 1.3; // significantly higher range

        public override double SpecialEffectProbability => 0.1;

        protected override ProtoSkillWeapons WeaponSkill => GetSkill<SkillWeaponsConventional>();

        protected override WeaponFirePatternPreset PrepareFirePatternPreset()
        {
            return new WeaponFirePatternPreset(
                initialSequence: new[] { 0.0, 0.4, -0.4 },
                cycledSequence: new[] { 0.5, -0.3, 0.3, -0.5, 0.0 });
        }

        protected override WeaponFireTracePreset PrepareFireTracePreset()
        {
            return WeaponFireTracePresets.HeavySniper;
        }

        protected override void PrepareMuzzleFlashDescription(MuzzleFlashDescription description)
        {
            description.Set(MuzzleFlashPresets.ModernRifle)
                       .Set(textureScreenOffset: (47, 6));
        }

        protected override void PrepareProtoWeaponRanged(
            out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
            ref DamageDescription overrideDamageDescription)
        {
            compatibleAmmoProtos = GetAmmoOfType<IAmmoCaliber10mm>();
        }

        protected override ReadOnlySoundPreset<WeaponSound> PrepareSoundPresetWeapon()
        {
            return WeaponsSoundPresets.WeaponRangedLightRifle;
        }

        protected override void ServerOnSpecialEffect(ICharacter damagedCharacter, double damage)
        {
            ServerWeaponSpecialEffectsHelper.OnFirearmHit(damagedCharacter, damage);
        }
    }
}