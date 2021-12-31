namespace AtomicTorch.CBND.CoreMod.Items.Ammo
{
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Explosives;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Weapons;

    public class ItemAmmoShellHeavy : ProtoItemGrenade, IAmmoCannonShell
    {
        public override double DamageRadius => 2.1;

        public override string Description =>
            "High-explosive artillery rounds are highly effective against vehicle armor.";

        public override double FireRangeMax => 10;

        public override bool IsReferenceAmmo => true;

        public override ushort MaxItemsPerStack => ItemStackSize.Small;

        public override string Name => "Artillery shell";

        protected override void PrepareDamageDescriptionCharacters(
            out double damageValue,
            out double armorPiercingCoef,
            out double finalDamageMultiplier,
            DamageDistribution damageDistribution)
        {
            damageValue = 100;
            armorPiercingCoef = 0.8;
            finalDamageMultiplier = 1;
            damageDistribution.Set(DamageType.Explosion, 1);
        }

        protected override void PrepareDamageDescriptionStructures(
            out double damageValue,
            out double defencePenetrationCoef)
        {
            damageValue = 800;
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
            damagedCharacter.ServerAddStatusEffect<StatusEffectBleeding>(intensity: 0.2); // 2 minutes

            // 100% chance to add dazed
            damagedCharacter.ServerAddStatusEffect<StatusEffectDazed>(intensity: 0.1); // 1 second
        }
    }
}