﻿namespace AtomicTorch.CBND.CoreMod.Items.Ammo
{
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class ItemAmmo300ArmorPiercing : ProtoItemAmmo, IAmmoCaliber300
    {
        public override string Description =>
            "Heavy anti-material .300 rounds with armor-piercing capabilities.";

        public override bool IsReferenceAmmo => true;

        public override string Name => ".300 armor-piercing ammo";

        public override void ServerOnCharacterHit(ICharacter damagedCharacter, double damage, ref bool isDamageStop)
        {
            if (damage < 1)
            {
                return;
            }

            // 40% chance to add bleeding
            if (RandomHelper.RollWithProbability(0.40))
            {
                damagedCharacter.ServerAddStatusEffect<StatusEffectBleeding>(intensity: 0.05); // 30 seconds
            }

            // 2% chance
            if (RandomHelper.RollWithProbability(0.02))
            {
                damagedCharacter.ServerAddStatusEffect<StatusEffectDazed>(
                    intensity: 1.0 / StatusEffectDazed.MaxDuration); // 1 second
                // that's OP, commented out temporary
                //damagedCharacter.ServerAddStatusEffect<StatusEffectBrokenLeg>(intensity: 1); // permanent
            }
        }

        protected override void PrepareDamageDescription(
            out double damageValue,
            out double armorPiercingCoef,
            out double finalDamageMultiplier,
            out double rangeMax,
            DamageDistribution damageDistribution)
        {
            damageValue = 30;
            armorPiercingCoef = 0.5;
            finalDamageMultiplier = 1;
            rangeMax = 10;
            damageDistribution.Set(DamageType.Kinetic, 1);
        }

        protected override WeaponFireTracePreset PrepareFireTracePreset()
        {
            return WeaponFireTracePresets.Heavy;
        }
    }
}