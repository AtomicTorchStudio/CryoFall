namespace AtomicTorch.CBND.CoreMod.Items.Weapons.MobWeapons
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class ItemWeaponMobPragmiumQueenRanged : ProtoItemMobWeaponRangedNoAim
    {
        private static readonly double[] DamageRayAngleOffets
            = { -12, -6, 0, 6, 12 };

        public override double DamageApplyDelay => 0.4;

        public override double FireAnimationDuration => 0.9;

        public override double FireInterval => 2.0;

        public override (float min, float max) SoundPresetWeaponDistance
            => (15, 45);

        public override (float min, float max) SoundPresetWeaponDistance3DSpread
            => (10, 35);

        public override string WeaponAttachmentName => "Head";

        public override double SharedUpdateAndGetFirePatternCurrentSpreadAngleDeg(WeaponState state)
        {
            // angle variation within 25 degrees
            return 25 * (RandomHelper.NextDouble() - 0.5);
        }

        protected override WeaponFireScatterPreset PrepareFireScatterPreset()
        {
            return new(DamageRayAngleOffets);
        }

        protected override WeaponFireTracePreset PrepareFireTracePreset()
        {
            return WeaponFireTracePresets.MobPragmiumQueen;
        }

        protected override void PrepareProtoWeapon(
            out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
            ref DamageDescription overrideDamageDescription)
        {
            // no ammo used
            compatibleAmmoProtos = null;

            var damageDistribution = new DamageDistribution()
                .Set(DamageType.Cold, 1.0);

            overrideDamageDescription = new DamageDescription(
                // each ray deals 15 dmg, generally player is hit up by 1-3 of them
                damageValue: DamageRayAngleOffets.Length * 15,
                armorPiercingCoef: 0.1,
                finalDamageMultiplier: 1.25,
                rangeMax: 15.5,
                damageDistribution: damageDistribution);
        }

        protected override ReadOnlySoundPreset<WeaponSound> PrepareSoundPresetWeapon()
        {
            return new SoundPreset<WeaponSound>()
                .Add(WeaponSound.Shot, "Skeletons/PragmiumQueen/Weapon/ShotRanged");
        }
    }
}