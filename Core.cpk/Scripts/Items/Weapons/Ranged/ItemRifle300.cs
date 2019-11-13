namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Weapons;

    public class ItemRifle300 : ProtoItemWeaponRanged
    {
        public override ushort AmmoCapacity => 6;

        public override double AmmoReloadDuration => 3;

        public override double CharacterAnimationAimingRecoilDuration => 0.5;

        public override double CharacterAnimationAimingRecoilPower => 1.3;

        public override double DamageMultiplier => 1.25; // higher damage

        public override string Description => "Heavy anti-material rifle developed for high-power .300 rounds.";

        public override uint DurabilityMax => 360;

        public override double FireInterval => 0.9; // slow firing rate

        public override string Name => "Heavy rifle";

        public override double RangeMultipier => 1.3; // significantly higher range

        public override double SpecialEffectProbability => 0.2;

        protected override ProtoSkillWeapons WeaponSkill => GetSkill<SkillWeaponsConventional>();

        protected override WeaponFireTracePreset PrepareFireTracePreset()
        {
            return WeaponFireTracePresets.HeavySniper;
        }

        protected override void PrepareMuzzleFlashDescription(MuzzleFlashDescription description)
        {
            description.Set(MuzzleFlashPresets.ModernRifle)
                       .Set(textureScreenOffset: (62, 5));
        }

        protected override void PrepareProtoWeaponRanged(
            out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
            ref DamageDescription overrideDamageDescription)
        {
            compatibleAmmoProtos = GetAmmoOfType<IAmmoCaliber300>();
        }

        protected override ReadOnlySoundPreset<WeaponSound> PrepareSoundPresetWeapon()
        {
            return WeaponsSoundPresets.WeaponRangedSniperRifle;
        }

        protected override void ServerOnSpecialEffect(ICharacter damagedCharacter, double damage)
        {
            ServerWeaponSpecialEffectsHelper.OnFirearmHit(damagedCharacter, damage);
        }
    }
}