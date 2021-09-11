namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Weapons;

    /// <summary>
    /// Flintlock pistol has two charges compared to musket. Otherwise it is inferior.
    /// But those two charges make it much more convenient.
    /// </summary>
    public class ItemFlintlockPistol : ProtoItemWeaponRanged
    {
        public override ushort AmmoCapacity => 2;

        public override double AmmoReloadDuration => 4;

        public override string CharacterAnimationAimingName => "WeaponPistolAiming";

        public override double CharacterAnimationAimingRecoilDuration => 0.5;

        public override string CharacterAnimationAimingRecoilName => "WeaponPistolShooting";

        public override double CharacterAnimationAimingRecoilPower => 1.2;

        public override string Description =>
            "Primitive pistol. Uses paper cartridges for ammo. Holds two charges, which can be fired very quickly.";

        public override uint DurabilityMax => 300;

        public override double FireInterval => 0.2; // can fire as soon as reloaded

        public override string Name => "Flintlock pistol";

        public override double ReadyDelayDuration => WeaponReadyDelays.ConventionalPistols;

        public override double SpecialEffectProbability => 0.25;

        protected override ProtoSkillWeapons WeaponSkill => GetSkill<SkillWeaponsConventional>();

        protected override WeaponFirePatternPreset PrepareFirePatternPreset()
        {
            return new(
                initialSequence: new[] { 0.0 },
                cycledSequence: new[] { 2.5 });
        }

        protected override void PrepareMuzzleFlashDescription(MuzzleFlashDescription description)
        {
            description.Set(MuzzleFlashPresets.PrimitivePistol)
                       .Set(textureScreenOffset: (2, 7));
        }

        protected override void PrepareProtoWeaponRanged(
            out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
            ref DamageDescription overrideDamageDescription)
        {
            compatibleAmmoProtos = GetAmmoOfType<IAmmoPaperCartrige>();
        }

        protected override ReadOnlySoundPreset<WeaponSound> PrepareSoundPresetWeapon()
        {
            return WeaponsSoundPresets.WeaponRangedFlintlockPistol;
        }

        protected override void ServerOnSpecialEffect(ICharacter damagedCharacter, double damage)
        {
            ServerWeaponSpecialEffectsHelper.OnFirearmHit(damagedCharacter, damage);
        }
    }
}