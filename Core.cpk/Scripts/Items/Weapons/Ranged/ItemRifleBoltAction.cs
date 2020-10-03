namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Weapons;

    public class ItemRifleBoltAction : ProtoItemWeaponRanged
    {
        public override ushort AmmoCapacity => 5;

        public override double AmmoReloadDuration => 3;

        public override double CharacterAnimationAimingRecoilDuration => 0.5;

        public override double CharacterAnimationAimingRecoilPower => 1.2;

        public override double DamageMultiplier => 1.2; // higher damage

        public override string Description => "Primitive bolt-action rifle developed for commonly used 8mm ammunition.";

        public override uint DurabilityMax => 200;

        public override double FireInterval => 0.7; // slower firing rate

        public override string Name => "Bolt-action rifle";

        public override double RangeMultiplier => 1.3; // significantly higher range

        public override double SpecialEffectProbability => 0.2;

        protected override ProtoSkillWeapons WeaponSkill => GetSkill<SkillWeaponsConventional>();

        protected override WeaponFireTracePreset PrepareFireTracePreset()
        {
            return WeaponFireTracePresets.HeavySniper;
        }

        protected override void PrepareMuzzleFlashDescription(MuzzleFlashDescription description)
        {
            description.Set(MuzzleFlashPresets.SimplePistol)
                       .Set(textureScreenOffset: (58, 9));
        }

        protected override void PrepareProtoWeaponRanged(
            out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
            ref DamageDescription overrideDamageDescription)
        {
            compatibleAmmoProtos = GetAmmoOfType<IAmmoCaliber8mm>();
        }

        protected override ReadOnlySoundPreset<WeaponSound> PrepareSoundPresetWeapon()
        {
            return WeaponsSoundPresets.WeaponRangedBoltActionRifle;
        }

        protected override void ServerOnSpecialEffect(ICharacter damagedCharacter, double damage)
        {
            ServerWeaponSpecialEffectsHelper.OnFirearmHit(damagedCharacter, damage);
        }
    }
}