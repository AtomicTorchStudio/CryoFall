namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Weapons;

    public class ItemLuger : ProtoItemWeaponRanged
    {
        public override ushort AmmoCapacity => 8;

        public override double AmmoReloadDuration => 2;

        public override string CharacterAnimationAimingName => "WeaponPistolAiming";

        public override string CharacterAnimationAimingRecoilName => "WeaponPistolShooting";

        public override string Description =>
            "Semi-automatic pistol of simple design. Relatively cheap to produce compared to modern firearms.";

        public override uint DurabilityMax => 360; // slightly lower

        public override double FireInterval => 0.5;

        public override string Name => "Luger";

        public override double RangeMultiplier => 1.1; // slightly higher

        public override double ReadyDelayDuration => WeaponReadyDelays.ConventionalPistols;

        public override double SpecialEffectProbability => 0.25;

        protected override ProtoSkillWeapons WeaponSkill => GetSkill<SkillWeaponsConventional>();

        protected override WeaponFirePatternPreset PrepareFirePatternPreset()
        {
            return new(
                initialSequence: new[] { 0.0, 0.5, 0.5 },
                cycledSequence: new[] { 1.2, 0.6, 1.2, 0.6 });
        }

        protected override void PrepareMuzzleFlashDescription(MuzzleFlashDescription description)
        {
            description.Set(MuzzleFlashPresets.SimplePistol)
                       .Set(textureScreenOffset: (-14, 6));
        }

        protected override void PrepareProtoWeaponRanged(
            out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
            ref DamageDescription overrideDamageDescription)
        {
            compatibleAmmoProtos = GetAmmoOfType<IAmmoCaliber8mm>();
        }

        protected override ReadOnlySoundPreset<WeaponSound> PrepareSoundPresetWeapon()
        {
            return WeaponsSoundPresets.WeaponRangedLuger;
        }

        protected override void ServerOnSpecialEffect(ICharacter damagedCharacter, double damage)
        {
            ServerWeaponSpecialEffectsHelper.OnFirearmHit(damagedCharacter, damage);
        }
    }
}