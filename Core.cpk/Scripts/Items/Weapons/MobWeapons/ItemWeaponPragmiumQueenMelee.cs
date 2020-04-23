namespace AtomicTorch.CBND.CoreMod.Items.Weapons.MobWeapons
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class ItemWeaponPragmiumQueenMelee : ProtoItemMobWeaponMelee
    {
        private static readonly double[] DamageRayAngleOffets
            = { -30, -25, -20, -15, -10, -5, 0, 5, 10, 15, 20, 25, 30 };

        public override double DamageApplyDelay => 0.3;

        public override double FireAnimationDuration => 0.8;

        public override double FireInterval => 1.5;

        public override (float min, float max) SoundPresetWeaponDistance
            => (15, 45);

        public override (float min, float max) SoundPresetWeaponDistance3DSpread
            => (10, 35);

        protected override WeaponFireScatterPreset PrepareFireScatterPreset()
        {
            return new WeaponFireScatterPreset(DamageRayAngleOffets);
        }

        protected override void PrepareProtoWeapon(
            out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
            ref DamageDescription overrideDamageDescription)
        {
            // no ammo used
            compatibleAmmoProtos = null;

            var damageDistribution = new DamageDistribution()
                .Set(DamageType.Impact, 1.0);

            overrideDamageDescription = new DamageDescription(
                // each ray deals 10 dmg, generally player is hit up by 3-4 of them
                damageValue: DamageRayAngleOffets.Length * 10,
                armorPiercingCoef: 0.25,
                finalDamageMultiplier: 1.25,
                rangeMax: 4.1,
                damageDistribution: damageDistribution);
        }

        protected override ReadOnlySoundPreset<WeaponSound> PrepareSoundPresetWeapon()
        {
            return new SoundPreset<WeaponSound>()
                .Add(WeaponSound.Shot, "Skeletons/PragmiumQueen/Weapon/ShotMelee");
        }

        protected override void ServerOnSpecialEffect(ICharacter damagedCharacter, double damage)
        {
            if (RandomHelper.RollWithProbability(0.7))
            {
                damagedCharacter.ServerAddStatusEffect<StatusEffectBleeding>(intensity: 0.1);
                damagedCharacter.ServerAddStatusEffect<StatusEffectPain>(intensity: 0.4);
            }

            if (RandomHelper.RollWithProbability(0.25))
            {
                damagedCharacter.ServerAddStatusEffect<StatusEffectToxins>(intensity: 0.2);
            }
        }
    }
}