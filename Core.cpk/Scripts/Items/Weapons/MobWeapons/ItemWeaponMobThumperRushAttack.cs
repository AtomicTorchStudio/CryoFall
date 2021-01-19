namespace AtomicTorch.CBND.CoreMod.Items.Weapons.MobWeapons
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.GameApi.Data.Weapons;

    public class ItemWeaponMobThumperRushAttack : ItemWeaponMobThumperNormalAttack
    {
        public override double DamageApplyDelay => 0.15;

        public override double FireAnimationDuration => 0.75;

        protected override void PrepareProtoWeapon(
            out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
            ref DamageDescription overrideDamageDescription)
        {
            // no ammo used
            compatibleAmmoProtos = null;

            overrideDamageDescription = new DamageDescription(
                // each ray deals 6 dmg (higher than normal) but player is hit only by a few of them
                damageValue: DamageRayAngleOffets.Length * 6,
                armorPiercingCoef: 0.4,
                finalDamageMultiplier: 1.33,
                rangeMax: 3.0,
                damageDistribution: new DamageDistribution(DamageType.Impact, 1));
        }
    }
}