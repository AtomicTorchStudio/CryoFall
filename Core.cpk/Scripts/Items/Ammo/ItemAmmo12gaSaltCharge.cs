namespace AtomicTorch.CBND.CoreMod.Items.Ammo
{
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Weapons;

    public class ItemAmmo12gaSaltCharge : ProtoItemAmmo, IAmmoCaliber12g
    {
        public override string Description =>
            "These shells are filled with coarse salt and are primarily designed to scare intruders and thieves, rather than for actual combat. They do very little damage, but if you get hit...the pain will be unthinkable.";

        public override bool IsSuppressWeaponSpecialEffect => true;

        public override string Name => "12-gauge salt charge";

        public override void ServerOnCharacterHit(ICharacter damagedCharacter, double damage)
        {
            if (damage > 1)
            {
                // if it was able to inflict any real damage - add full pain
                damagedCharacter.ServerAddStatusEffect<StatusEffectPain>(intensity: 1);
            }
        }

        protected override void PrepareDamageDescription(
            out double damageValue,
            out double armorPiercingCoef,
            out double finalDamageMultiplier,
            out double rangeMax,
            DamageDistribution damageDistribution)
        {
            damageValue = 6;
            armorPiercingCoef = 0;
            finalDamageMultiplier = 2;
            rangeMax = 7;

            damageDistribution.Set(DamageType.Impact, 1.0);
        }
    }
}