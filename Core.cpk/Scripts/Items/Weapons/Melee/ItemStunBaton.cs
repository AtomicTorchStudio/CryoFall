namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Melee
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using static SoundPresets.ObjectMaterial;

    public class ItemStunBaton : ProtoItemWeaponMelee
    {
        public override double DamageApplyDelay => 0.075;

        public override string Description => "Uses electricity from built-in batteries to stun opponents.";

        public override uint DurabilityMax => 100;

        public override double FireAnimationDuration => 0.6;

        public override string Name => "Stun baton";

        public override double SpecialEffectProbability => 1.0; // 100%

        protected override ProtoSkillWeapons WeaponSkill => GetSkill<SkillWeaponsMelee>();

        protected override void PrepareProtoWeapon(
            out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
            ref DamageDescription overrideDamageDescription)
        {
            compatibleAmmoProtos = null;

            overrideDamageDescription = new DamageDescription(
                damageValue: 25,
                armorPiercingCoef: 0.6,
                finalDamageMultiplier: 1.0,
                rangeMax: 1.2,
                damageDistribution: new DamageDistribution(DamageType.Impact, 1));
        }

        protected override ReadOnlySoundPreset<ObjectMaterial> PrepareSoundPresetHit()
        {
            var path = "Hit/Special/StunBaton";
            return new SoundPreset<ObjectMaterial>(MaterialHitsSoundPresets.HitSoundDistancePreset)
                   .Add(SoftTissues, path)
                   .Add(HardTissues, path)
                   .Add(SolidGround, path)
                   .Add(Vegetation,  path)
                   .Add(Wood,        path)
                   .Add(Stone,       path)
                   .Add(Metal,       path)
                   .Add(Glass,       path);
        }

        protected override void ServerOnSpecialEffect(ICharacter damagedCharacter, double damage)
        {
            ServerWeaponSpecialEffectsHelper.OnStunBatonHit(damagedCharacter, damage);
        }
    }
}