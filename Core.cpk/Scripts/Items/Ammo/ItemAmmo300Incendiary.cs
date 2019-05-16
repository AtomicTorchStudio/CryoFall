namespace AtomicTorch.CBND.CoreMod.Items.Ammo
{
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class ItemAmmo300Incendiary : ProtoItemAmmo, IAmmoCaliber300
    {
        public override string Description =>
            "Heavy anti-material .300 incendiary rounds.";

        public override string Name => ".300 incendiary ammo";

        public override void ServerOnCharacterHit(ICharacter damagedCharacter, double damage)
        {
            // 40% chance to add bleeding
            if (RandomHelper.RollWithProbability(0.40))
            {
                damagedCharacter.ServerAddStatusEffect<StatusEffectBleeding>(intensity: 0.05); // 30 seconds
            }

            // guaranteed heat effect
            damagedCharacter.ServerAddStatusEffect<StatusEffectHeat>(intensity: 0.4);
        }

        protected override void PrepareDamageDescription(
            out double damageValue,
            out double armorPiercingCoef,
            out double finalDamageMultiplier,
            out double rangeMax,
            DamageDistribution damageDistribution)
        {
            damageValue = 20;
            armorPiercingCoef = 0.2;
            finalDamageMultiplier = 1.5;
            rangeMax = 11;

            damageDistribution.Set(DamageType.Kinetic, 0.6);
            damageDistribution.Set(DamageType.Heat,    0.4);
        }
    }
}