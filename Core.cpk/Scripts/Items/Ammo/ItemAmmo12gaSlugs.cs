namespace AtomicTorch.CBND.CoreMod.Items.Ammo
{
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Weapons;

    public class ItemAmmo12gaSlugs : ProtoItemAmmo, IAmmoCaliber12g
    {
        public override string Description =>
            "Slugs provide much higher stopping power than pellets and can be used against a variety of possible targets. Due to their high mass, the slugs are quite effective even against well-armored targets.";

        public override string Name => "12-gauge slug ammo";

        public override void ServerOnCharacterHit(ICharacter damagedCharacter, double damage)
        {
            damagedCharacter.ServerAddStatusEffect<StatusEffectDazed>(
                intensity: 0.4 / StatusEffectDazed.MaxDuration); // add effect for 0.4 seconds
        }

        protected override void PrepareDamageDescription(
            out double damageValue,
            out double armorPiercingCoef,
            out double finalDamageMultiplier,
            out double rangeMax,
            DamageDistribution damageDistribution)
        {
            damageValue = 36;
            armorPiercingCoef = 0;
            finalDamageMultiplier = 1.1;
            rangeMax = 9;

            damageDistribution.Set(DamageType.Kinetic, 0.7);
            damageDistribution.Set(DamageType.Impact,  0.3);
        }
    }
}