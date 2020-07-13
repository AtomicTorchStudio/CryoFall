namespace AtomicTorch.CBND.CoreMod.Items.Ammo
{
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Explosives;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Weapons;

    public class ItemAmmoGrenadeFragmentation : ProtoItemGrenade, IAmmoGrenadeForGrenadeLauncher
    {
        public override double DamageRadius => 4.1; // much higher radius due to shrapnel

        public override string Description =>
            "Special fragmentation grenade with extended effective range. Damages everything in a wide radius upon impact.";

        public override double FireRangeMax => 8;

        public override bool IsReferenceAmmo => false;

        public override ushort MaxItemsPerStack => ItemStackSize.Small;

        public override string Name => "Fragmentation grenade";

        protected override void PrepareDamageDescriptionCharacters(
            out double damageValue,
            out double armorPiercingCoef,
            out double finalDamageMultiplier,
            DamageDistribution damageDistribution)
        {
            damageValue = 60; // lower damage than other grenades due to higher radius and higher final multiplier
            armorPiercingCoef = 0;
            finalDamageMultiplier = 1.25;

            damageDistribution.Set(DamageType.Explosion, 1.0);
        }

        protected override void PrepareDamageDescriptionStructures(
            out double damageValue,
            out double defencePenetrationCoef)
        {
            damageValue = 300;
            defencePenetrationCoef = 0;
        }

        protected override void PrepareExplosionPreset(out ExplosionPreset explosionPreset)
        {
            explosionPreset = ExplosionPresets.Grenade;
        }

        protected override WeaponFireTracePreset PrepareFireTracePreset()
        {
            return WeaponFireTracePresets.Grenade;
        }

        protected override void ServerOnCharacterHitByExplosion(
            ICharacter damagedCharacter,
            double damage,
            WeaponFinalCache weaponCache)
        {
            if (damage < 1)
            {
                return;
            }

            // 100% chance to add bleeding
            damagedCharacter.ServerAddStatusEffect<StatusEffectBleeding>(intensity: 0.05);
        }
    }
}