namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Weapons;

    public class ItemSubmachinegun10mm : ProtoItemWeaponRanged
    {
        public override ushort AmmoCapacity => 20;

        public override double AmmoReloadDuration => 2;

        public override double CharacterAnimationAimingRecoilDuration => 0.3;

        public override double CharacterAnimationAimingRecoilPower => 0.667;

        public override double CharacterAnimationAimingRecoilPowerAddCoef
            => 1 / 2.5; // full recoil power will be gained on third shot

        public override double DamageMultiplier => 0.8; // slightly lower than default

        public override string Description => "Submachine gun developed for 10mm rounds.";

        public override uint DurabilityMax => 400;

        public override double FireInterval => 1 / 10d; // 10 per second

        public override string Name => "Submachine gun";

        public override double SpecialEffectProbability => 0.1;

        protected override ProtoSkillWeapons WeaponSkill => GetSkill<SkillWeaponsConventional>();

        protected override WeaponFirePatternPreset PrepareFirePatternPreset()
        {
            return new WeaponFirePatternPreset(
                initialSequence: new[] { 0.0, 1.0, -2.0 },
                cycledSequence: new[] { 1.5, 3.0, 2.5, 0.0, 3.5, -1.5, -3.0, -2.5, 0.0, -3.5 });
        }

        protected override void PrepareMuzzleFlashDescription(MuzzleFlashDescription description)
        {
            description.Set(MuzzleFlashPresets.ModernSubmachinegun)
                       .Set(textureScreenOffset: (9, 19));
        }

        protected override void PrepareProtoWeaponRanged(
            out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
            ref DamageDescription overrideDamageDescription)
        {
            compatibleAmmoProtos = GetAmmoOfType<IAmmoCaliber10mm>();
        }

        protected override ReadOnlySoundPreset<WeaponSound> PrepareSoundPresetWeapon()
        {
            return WeaponsSoundPresets.WeaponRangedSMG;
        }

        protected override void ServerOnSpecialEffect(ICharacter damagedCharacter, double damage)
        {
            ServerWeaponSpecialEffectsHelper.OnFirearmHit(damagedCharacter, damage);
        }
    }
}