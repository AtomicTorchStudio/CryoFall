namespace AtomicTorch.CBND.CoreMod.Items.Ammo
{
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Explosives;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Weapons;

    public class ItemAmmoGrenadeFreeze : ProtoItemGrenade, IAmmoGrenadeForGrenadeLauncher
    {
        public override double DamageRadius => 2.1;

        public override string Description =>
            "Special-purpose grenade that disperses liquid helium imbued with pragmium particles upon impact to freeze the target.";

        public override double FireRangeMax => 8;

        public override bool IsReferenceAmmo => false;

        public override ushort MaxItemsPerStack => ItemStackSize.Small;

        public override string Name => "Helium grenade";

        protected override void PrepareDamageDescriptionCharacters(
            out double damageValue,
            out double armorPiercingCoef,
            out double finalDamageMultiplier,
            DamageDistribution damageDistribution)
        {
            damageValue = 95;
            armorPiercingCoef = 0.2;
            finalDamageMultiplier = 1;

            damageDistribution.Set(DamageType.Explosion, 0.6)
                              .Set(DamageType.Cold, 0.4);
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
            explosionPreset = ExplosionPresets.GrenadePragmium;
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
            damagedCharacter.ServerAddStatusEffect<StatusEffectBleeding>(intensity: 0.05); // 30 seconds

            // 100% chance to add dazed
            damagedCharacter.ServerAddStatusEffect<StatusEffectDazed>(intensity: 0.5); // 2 seconds
        }
    }
}