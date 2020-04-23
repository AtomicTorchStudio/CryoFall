namespace AtomicTorch.CBND.CoreMod.Items.Weapons.MobWeapons
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class ItemWeaponLizardFangs : ProtoItemMobWeaponMelee
    {
        public override double FireAnimationDuration => 0.6;

        public override double FireInterval => this.FireAnimationDuration * 1.5;

        protected override void PrepareProtoWeapon(
            out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
            ref DamageDescription overrideDamageDescription)
        {
            // no ammo used
            compatibleAmmoProtos = null;

            var damageDistribution = new DamageDistribution()
                                     .Set(DamageType.Impact,   0.8)
                                     .Set(DamageType.Chemical, 0.2); // uses poison or something :)

            overrideDamageDescription = new DamageDescription(
                damageValue: 22,
                armorPiercingCoef: 0,
                finalDamageMultiplier: 1.25,
                rangeMax: 1.5,
                damageDistribution: damageDistribution);
        }

        protected override void ServerOnSpecialEffect(ICharacter damagedCharacter, double damage)
        {
            if (RandomHelper.RollWithProbability(0.25))
            {
                damagedCharacter.ServerAddStatusEffect<StatusEffectBleeding>(intensity: 0.1);
                damagedCharacter.ServerAddStatusEffect<StatusEffectPain>(intensity: 0.2);
            }

            if (RandomHelper.RollWithProbability(0.25))
            {
                damagedCharacter.ServerAddStatusEffect<StatusEffectToxins>(intensity: 0.2);
            }
        }
    }
}