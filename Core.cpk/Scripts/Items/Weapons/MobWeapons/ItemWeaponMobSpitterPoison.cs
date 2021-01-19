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

    public class ItemWeaponMobSpitterPoison : ProtoItemMobWeaponRanged
    {
        public override string CharacterAnimationAimingName => "Aim2";

        public override double CharacterAnimationAimingRecoilDuration => 0.4;

        public override string CharacterAnimationAimingRecoilName => "AimRecoil";

        public override double CharacterAnimationAimingRecoilPower => 0.667;

        public override double DamageApplyDelay => 0.2;

        public override double FireAnimationDuration => 0.3;

        public override double FireInterval => 1.5;

        public override string WeaponAttachmentName => "Head";

        public override string GetCharacterAnimationNameFire(ICharacter character)
        {
            return "Shot";
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
            // angle variation within 20 degrees
            return 20 * (RandomHelper.NextDouble() - 0.5);
        }

        protected override WeaponFireTracePreset PrepareFireTracePreset()
        {
            return WeaponFireTracePresets.MobPoisonBig;
        }

        protected override void PrepareProtoWeaponRanged(
            out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
            ref DamageDescription overrideDamageDescription)
        {
            // no ammo used
            compatibleAmmoProtos = null;

            var damageDistribution = new DamageDistribution()
                                     .Set(DamageType.Heat,     0.4)
                                     .Set(DamageType.Chemical, 0.6);

            overrideDamageDescription = new DamageDescription(
                damageValue: 13,
                armorPiercingCoef: 0.25,
                finalDamageMultiplier: 1.0,
                rangeMax: 14, // a bit larger than a heavy rifle due to the monster larger size
                damageDistribution: damageDistribution);
        }
    }
}