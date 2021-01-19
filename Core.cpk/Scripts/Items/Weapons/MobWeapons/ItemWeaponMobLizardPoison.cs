namespace AtomicTorch.CBND.CoreMod.Items.Weapons.MobWeapons
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class ItemWeaponMobLizardPoison : ProtoItemMobWeaponRangedNoAim
    {
        public override double DamageApplyDelay => 0.2;

        public override double FireAnimationDuration => 0.7;

        public override double FireInterval => 2.5;

        public override string WeaponAttachmentName => "Head";

        public override string GetCharacterAnimationNameFire(ICharacter character)
        {
            return GetMeleeCharacterAnimationNameFire(character);
        }

        public override void SharedOnHit(
            WeaponFinalCache weaponCache,
            IWorldObject damagedObject,
            double damage,
            WeaponHitData hitData,
            out bool isDamageStop)
        {
            base.SharedOnHit(weaponCache,
                             damagedObject,
                             damage,
                             hitData,
                             out isDamageStop);

            if (IsServer
                && damage > 0
                && damagedObject is ICharacter damagedCharacter)
            {
                // guaranteed small toxin effect per hit
                // (please note the addition formula inside the Toxins effect class is NOT linear
                //  so subsequent hits increasing it on less than 20%)
                damagedCharacter.ServerAddStatusEffect<StatusEffectToxins>(intensity: 0.2);
            }
        }

        public override double SharedUpdateAndGetFirePatternCurrentSpreadAngleDeg(WeaponState state)
        {
            // angle variation within 30 degrees
            return 30 * (RandomHelper.NextDouble() - 0.5);
        }

        protected override WeaponFireTracePreset PrepareFireTracePreset()
        {
            return WeaponFireTracePresets.MobPoison;
        }

        protected override void PrepareProtoWeapon(
            out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
            ref DamageDescription overrideDamageDescription)
        {
            // no ammo used
            compatibleAmmoProtos = null;

            var damageDistribution = new DamageDistribution()
                                     .Set(DamageType.Heat,     0.6)
                                     .Set(DamageType.Chemical, 0.4);

            overrideDamageDescription = new DamageDescription(
                damageValue: 13,
                armorPiercingCoef: 0,
                finalDamageMultiplier: 1.25,
                rangeMax: 7,
                damageDistribution: damageDistribution);
        }
    }
}