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

    public class ItemWeaponGenericAnimalWeak : ProtoItemMobWeaponMelee
    {
        public override double FireAnimationDuration => 0.6;

        public override double FireInterval => 1.5;

        protected override WeaponFireTracePreset PrepareFireTracePreset()
        {
            return WeaponFireTracePresets.NoWeapon;
        }

        protected override void PrepareProtoWeapon(
            out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
            ref DamageDescription overrideDamageDescription)
        {
            // no ammo used
            compatibleAmmoProtos = null;

            overrideDamageDescription = new DamageDescription(
                damageValue: 10,
                armorPiercingCoef: 0,
                finalDamageMultiplier: 1,
                rangeMax: 1.5,
                damageDistribution: new DamageDistribution(DamageType.Impact, 1));
        }

        protected override ReadOnlySoundPreset<ObjectMaterial> PrepareSoundPresetHit()
        {
            return MaterialHitsSoundPresets.MeleeNoWeapon;
        }

        protected override void ServerOnSpecialEffect(ICharacter damagedCharacter, double damage)
        {
            if (RandomHelper.RollWithProbability(0.05))
            {
                damagedCharacter.ServerAddStatusEffect<StatusEffectBleeding>(intensity: 0.1);
            }
        }
    }
}