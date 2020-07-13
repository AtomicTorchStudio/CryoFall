namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Weapons;

    public class ItemHandgun10mm : ProtoItemWeaponRanged
    {
        public override ushort AmmoCapacity => 12;

        public override double AmmoReloadDuration => 2;

        public override string CharacterAnimationAimingName => "WeaponPistolAiming";

        public override string CharacterAnimationAimingRecoilName => "WeaponPistolShooting";

        public override double DamageMultiplier => 1.1; // slightly higher

        public override string Description => "Handgun developed for 10mm rounds.";

        public override uint DurabilityMax => 250;

        public override double FireInterval => 0.5;

        public override string Name => "Handgun";

        public override double ReadyDelayDuration => 0.6;

        public override double SpecialEffectProbability => 0.25;

        protected override ProtoSkillWeapons WeaponSkill => GetSkill<SkillWeaponsConventional>();

        protected override WeaponFirePatternPreset PrepareFirePatternPreset()
        {
            return new WeaponFirePatternPreset(
                initialSequence: new[] { 0.0, 0.0, -0.5, 0.5 },
                cycledSequence: new[] { -1.0, 1.0, 0.0 });
        }

        protected override void PrepareMuzzleFlashDescription(MuzzleFlashDescription description)
        {
            description.Set(MuzzleFlashPresets.ModernHandgun)
                       .Set(textureScreenOffset: (-13, 9));
        }

        protected override void PrepareProtoWeaponRanged(
            out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
            ref DamageDescription overrideDamageDescription)
        {
            compatibleAmmoProtos = GetAmmoOfType<IAmmoCaliber10mm>();
        }

        protected override ReadOnlySoundPreset<WeaponSound> PrepareSoundPresetWeapon()
        {
            return WeaponsSoundPresets.WeaponRangedPistol;
        }

        protected override void ServerOnSpecialEffect(ICharacter damagedCharacter, double damage)
        {
            ServerWeaponSpecialEffectsHelper.OnFirearmHit(damagedCharacter, damage);
        }
    }
}