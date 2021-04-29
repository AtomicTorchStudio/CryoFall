namespace AtomicTorch.CBND.CoreMod.Items.Ammo
{
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Explosives;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ItemAmmoGrenadeFragmentation : ProtoItemGrenade, IAmmoGrenadeForGrenadeLauncher
    {
        public override double DamageRadius => 3.1; // much higher radius (more than x2 area size) due to shrapnel

        public override string Description =>
            "Special fragmentation grenade with extended effective range. Damages everything in a wide radius upon impact.";

        public override double FireRangeMax => 8;

        public override bool IsReferenceAmmo => false;

        public override ushort MaxItemsPerStack => ItemStackSize.Small;

        public override string Name => "Fragmentation grenade";

        protected override void ClientExplodeAt(
            IProtoItemWeapon protoWeapon,
            Vector2D shotSourcePosition,
            Vector2D explosionWorldPosition)
        {
            var timeToHit = WeaponSystemClientDisplay.SharedCalculateTimeToHit(protoWeapon.FireTracePreset
                                                                               ?? this.FireTracePreset,
                                                                               shotSourcePosition,
                                                                               explosionWorldPosition);

            ClientTimersSystem.AddAction(
                timeToHit,
                () =>
                {
                    SharedExplosionHelper.ClientExplode(explosionWorldPosition,
                                                        this.ExplosionPreset,
                                                        this.VolumeExplosion);

                    // add more explosions in the X pattern as it's a fragmentation grenade
                    ClientTimersSystem.AddAction(
                        0.2,
                        () =>
                        {
                            SharedExplosionHelper.ClientExplode(explosionWorldPosition + (1, 1),
                                                                this.ExplosionPreset,
                                                                volume: 0);

                            SharedExplosionHelper.ClientExplode(explosionWorldPosition + (1, -1),
                                                                this.ExplosionPreset,
                                                                volume: 0);

                            SharedExplosionHelper.ClientExplode(explosionWorldPosition + (-1, 1),
                                                                this.ExplosionPreset,
                                                                volume: 0);

                            SharedExplosionHelper.ClientExplode(explosionWorldPosition + (-1, -1),
                                                                this.ExplosionPreset,
                                                                volume: 0);
                        });
                });
        }

        protected override void PrepareDamageDescriptionCharacters(
            out double damageValue,
            out double armorPiercingCoef,
            out double finalDamageMultiplier,
            DamageDistribution damageDistribution)
        {
            damageValue = 65; // lower damage than other grenades due to higher radius and higher final multiplier
            armorPiercingCoef = 0;
            finalDamageMultiplier = 1.3;

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