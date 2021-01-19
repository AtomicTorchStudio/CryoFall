namespace AtomicTorch.CBND.CoreMod.Items.Weapons.MobWeapons
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Weapons;

    public class ItemWeaponMobThumperNormalAttack : ProtoItemMobWeaponMelee
    {
        protected static readonly double[] DamageRayAngleOffets
            = { -30, -25, -20, -15, -10, -5, 0, 5, 10, 15, 20, 25, 30 };

        public override double DamageApplyDelay => 0.5;

        public override double FireAnimationDuration => 1.0;

        public override double FireInterval => 2.0;

        protected override WeaponFireScatterPreset PrepareFireScatterPreset()
        {
            return new(DamageRayAngleOffets);
        }

        protected override void PrepareProtoWeapon(
            out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
            ref DamageDescription overrideDamageDescription)
        {
            // no ammo used
            compatibleAmmoProtos = null;

            overrideDamageDescription = new DamageDescription(
                // each ray deals 5 dmg but player is hit only by a few of them
                damageValue: DamageRayAngleOffets.Length * 5,
                armorPiercingCoef: 0.4,
                finalDamageMultiplier: 1.33,
                rangeMax: 2.2,
                damageDistribution: new DamageDistribution(DamageType.Impact, 1));
        }

        protected override ReadOnlySoundPreset<ObjectMaterial> PrepareSoundPresetHit()
        {
            return MaterialHitsSoundPresets.MeleeNoWeapon;
        }

        protected override void ServerOnSpecialEffect(ICharacter damagedCharacter, double damage)
        {
            /*if (RandomHelper.RollWithProbability(0.20))
            {
                damagedCharacter.ServerAddStatusEffect<StatusEffectBleeding>(intensity: 0.2);
                damagedCharacter.ServerAddStatusEffect<StatusEffectPain>(intensity: 0.1);
            }

            if (RandomHelper.RollWithProbability(0.05))
            {
                damagedCharacter.ServerAddStatusEffect<StatusEffectBrokenLeg>(intensity: 1);
                damagedCharacter.ServerAddStatusEffect<StatusEffectPain>(intensity: 0.2);
            }*/
        }
    }
}