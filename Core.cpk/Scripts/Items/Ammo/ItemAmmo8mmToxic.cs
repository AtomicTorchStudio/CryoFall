namespace AtomicTorch.CBND.CoreMod.Items.Ammo
{
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Weapons;

    public class ItemAmmo8mmToxic : ProtoItemAmmo, IAmmoCaliber8mm
    {
        public override string Description =>
            "Modified 8mm ammo with an enclosed cavity holding a small amount of concentrated toxin that bursts upon impact.";

        public override string Name => "8mm toxin ammo";

        public override void ServerOnCharacterHit(ICharacter damagedCharacter, double damage)
        {
            damagedCharacter.ServerAddStatusEffect<StatusEffectToxins>(intensity: 0.05); // 15 seconds
        }

        protected override void PrepareDamageDescription(
            out double damageValue,
            out double armorPiercingCoef,
            out double finalDamageMultiplier,
            out double rangeMax,
            DamageDistribution damageDistribution)
        {
            damageValue = 12;
            armorPiercingCoef = 0;
            finalDamageMultiplier = 2;
            rangeMax = 9;

            damageDistribution.Set(DamageType.Kinetic,  0.4);
            damageDistribution.Set(DamageType.Chemical, 0.6);
        }
    }
}