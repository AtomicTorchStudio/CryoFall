namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Weapons;

    public class ItemSteppenHawk : ProtoItemWeaponRanged
    {
        public override ushort AmmoCapacity => 7;

        public override double AmmoReloadDuration => 3;

        public override string CharacterAnimationAimingName => "WeaponPistolAiming";

        public override double CharacterAnimationAimingRecoilDuration => 0.66;

        public override string CharacterAnimationAimingRecoilName => "WeaponPistolShooting";

        public override double CharacterAnimationAimingRecoilPower => 1.5;

        public override string Description => "Semi-automatic handgun with extreme muzzle energy.";

        public override uint DurabilityMax => 360;

        public override double FireInterval => 0.9;

        public override string Name => "Steppen Hawk";

        public override double SpecialEffectProbability => 0.20;

        protected override ProtoSkillWeapons WeaponSkill => GetSkill<SkillWeaponsConventional>();

        protected override void PrepareMuzzleFlashDescription(MuzzleFlashDescription description)
        {
            description.Set(MuzzleFlashPresets.ModernRifle)
                       .Set(textureScreenOffset: (-4, 5));
        }

        protected override void PrepareProtoWeaponRanged(
            out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
            ref DamageDescription overrideDamageDescription)
        {
            compatibleAmmoProtos = GetAmmoOfType<IAmmoCaliber50>();
        }

        protected override ReadOnlySoundPreset<WeaponSound> PrepareSoundPresetWeapon()
        {
            return WeaponsSoundPresets.WeaponRangedMagnum;
        }

        protected override void ServerOnSpecialEffect(ICharacter damagedCharacter, double damage)
        {
            ServerWeaponSpecialEffectsHelper.OnFirearmHit(damagedCharacter, damage);
        }
    }
}