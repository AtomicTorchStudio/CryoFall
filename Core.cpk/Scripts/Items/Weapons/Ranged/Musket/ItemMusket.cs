namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Weapons;

    /// <summary>
    /// Musket has higher range and damage compared to flintlock pistol, but holds only one charge.
    /// </summary>
    public class ItemMusket : ProtoItemWeaponRanged
    {
        public override ushort AmmoCapacity => 1;

        public override double AmmoReloadDuration => 3;

        public override double CharacterAnimationAimingRecoilDuration => 0.7;

        public override double CharacterAnimationAimingRecoilPower => 1.5;

        public override double DamageMultiplier => 1.1;

        public override string Description =>
            "Old-school style musket. Uses paper cartridges. Offers better range and damage compared to flintlock pistol.";

        public override uint DurabilityMax => 240;

        public override double FireInterval => 0; // can fire as soon as reloaded

        public override string Name => "Musket";

        public override double RangeMultiplier => 1.1;

        public override double SpecialEffectProbability => 0.3;

        protected override ProtoSkillWeapons WeaponSkill => GetSkill<SkillWeaponsConventional>();

        protected override void PrepareMuzzleFlashDescription(MuzzleFlashDescription description)
        {
            description.Set(MuzzleFlashPresets.PrimitiveRifle)
                       .Set(textureScreenOffset: (332, 62));
        }

        protected override void PrepareProtoWeaponRanged(
            out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
            ref DamageDescription overrideDamageDescription)
        {
            compatibleAmmoProtos = GetAmmoOfType<IAmmoPaperCartrige>();
        }

        protected override ReadOnlySoundPreset<WeaponSound> PrepareSoundPresetWeapon()
        {
            return WeaponsSoundPresets.WeaponRangedMusket;
        }

        protected override void ServerOnSpecialEffect(ICharacter damagedCharacter, double damage)
        {
            ServerWeaponSpecialEffectsHelper.OnFirearmHit(damagedCharacter, damage);
        }
    }
}