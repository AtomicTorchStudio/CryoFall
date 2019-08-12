namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Weapons;

    public class ItemMachinePistol : ProtoItemWeaponRanged
    {
        public override ushort AmmoCapacity => 9;

        public override double AmmoReloadDuration => 2.5;

        // use pistol animation as this weapon is too short for the rifle animation
        public override string CharacterAnimationAimingName => "WeaponPistolAiming";

        public override double CharacterAnimationAimingRecoilDuration => 0.25;

        // small recoil - otherwise the character's hand moving wrong on the chassis due to the animation
        public override double CharacterAnimationAimingRecoilPower => 0.222;

        public override double CharacterAnimationAimingRecoilPowerAddCoef
            => 1 / 2.5; // full recoil power will be gained on third shot

        public override double DamageMultiplier => 0.7; // lower than default

        public override string Description => "Light machine pistol developed for 8mm rounds.";

        public override uint DurabilityMax => 400;

        public override double FireInterval => 1 / 10d; // 10 per second

        public override string Name => "Machine pistol";

        public override double SpecialEffectProbability => 0.1;

        protected override ProtoSkillWeapons WeaponSkill => GetSkill<SkillWeaponsConventional>();

        protected override void PrepareMuzzleFlashDescription(MuzzleFlashDescription description)
        {
            description.Set(MuzzleFlashPresets.PrimitivePistol)
                       .Set(textureScreenOffset: (0, 6));
        }

        protected override void PrepareProtoWeaponRanged(
            out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
            ref DamageDescription overrideDamageDescription)
        {
            compatibleAmmoProtos = GetAmmoOfType<IAmmoCaliber8mm>();
        }

        protected override ReadOnlySoundPreset<WeaponSound> PrepareSoundPresetWeapon()
        {
            return WeaponsSoundPresets.WeaponRangedMachinePistol;
        }

        protected override void ServerOnSpecialEffect(ICharacter damagedCharacter, double damage)
        {
            ServerWeaponSpecialEffectsHelper.OnFirearmHit(damagedCharacter, damage);
        }
    }
}