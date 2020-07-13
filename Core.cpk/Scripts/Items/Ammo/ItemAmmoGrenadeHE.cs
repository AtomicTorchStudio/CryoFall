namespace AtomicTorch.CBND.CoreMod.Items.Ammo
{
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Explosives;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Weapons;

    public class ItemAmmoGrenadeHE : ProtoItemGrenade, IAmmoGrenadeForGrenadeLauncher
    {
        public override double DamageRadius => 2.1;

        public override string Description =>
            "Standard high-explosive grenade that damages everything in a small radius at the point of impact.";

        public override double FireRangeMax => 8;

        public override bool IsReferenceAmmo => true;

        public override ushort MaxItemsPerStack => ItemStackSize.Small;

        public override string Name => "HE grenade";

        protected override void PrepareDamageDescriptionCharacters(
            out double damageValue,
            out double armorPiercingCoef,
            out double finalDamageMultiplier,
            DamageDistribution damageDistribution)
        {
            damageValue = 90;
            armorPiercingCoef = 0;
            finalDamageMultiplier = 1;

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
            damagedCharacter.ServerAddStatusEffect<StatusEffectBleeding>(intensity: 0.1);
        }
    }
}