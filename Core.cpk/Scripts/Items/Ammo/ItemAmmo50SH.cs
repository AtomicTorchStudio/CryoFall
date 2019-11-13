namespace AtomicTorch.CBND.CoreMod.Items.Ammo
{
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class ItemAmmo50SH : ProtoItemAmmo, IAmmoCaliber50
    {
        public override string Description => "High power large-caliber handgun cartridge.";

        public override string Name => ".50 SH ammo";

        public override void ServerOnCharacterHit(ICharacter damagedCharacter, double damage)
        {
            // 25% chance to add bleeding
            if (RandomHelper.RollWithProbability(0.25))
            {
                damagedCharacter.ServerAddStatusEffect<StatusEffectBleeding>(intensity: 0.10); // 1 minute
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
            armorPiercingCoef = 0.20;
            finalDamageMultiplier = 1.3;
            rangeMax = 10;

            damageDistribution.Set(DamageType.Kinetic, 1);
        }

        protected override WeaponFireTracePreset PrepareFireTracePreset()
        {
            return WeaponFireTracePresets.Firearm;
        }
    }
}