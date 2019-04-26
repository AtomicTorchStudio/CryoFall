namespace AtomicTorch.CBND.CoreMod.StaticObjects.Explosives.Bombs
{
    using System;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ObjectBombResonance : ProtoObjectExplosive
    {
        // damage radius to dynamic objects (like characters)
        private const double DamageRadiusDynamicObjectsOnly = 2.1;

        // damage radius to deliver full damage to static objects
        private const int DamageRadiusFullDamage = 4;

        // max damage radius to deliver damage to static objects
        // (see method ServerCalculateDamageCoefByDistance)
        private const int DamageRadiusMax = 7;

        public override double DamageRadius => DamageRadiusMax;

        public override bool ActivatesRaidModeForLandClaim => true;

        public override string Name => "Resonance bomb";

        public override double ServerCalculateDamageCoefByDistance(double distance)
        {
            var tileDistance = (int)distance;
            if (tileDistance <= DamageRadiusFullDamage)
            {
                return 1; // full damage
            }

            switch (tileDistance)
            {
                case 5:
                    return 0.7; // 70%
                case 6:
                    return 0.5; // 50%;
                case 7:
                    return 0.3; // 30%;

                // no damage beyond this point
                default:
                    return 0;
            }
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);
            data.ClientState.Renderer.DrawOrderOffsetY = 0.355;
        }

        protected override void PrepareDamageDescriptionCharacters(
            out double damageValue,
            out double armorPiercingCoef,
            out double finalDamageMultiplier,
            DamageDistribution damageDistribution)
        {
            damageValue = 120;
            armorPiercingCoef = 0;
            finalDamageMultiplier = 1;
            damageDistribution.Set(DamageType.Kinetic, 1);
        }

        protected override void PrepareDamageDescriptionStructures(
            out double damageValue,
            out double defencePenetrationCoef)
        {
            // Please not: while the game is the same as with the Modern bomb (T3)
            // it has a totally different damage propagation algorithm.
            // Resonance bombs are exploding like in bomberman—a cross-shaped damage propagation against walls and doors only
            // (as written in the tooltip). 4 closest tiles to the bomb are damaged through dealing 100% damage, then if there
            // is no free space it could damage through up to 7 tiles total (but the damage is reduced for every next tile after
            // fourth). It's extremely effective against multilayered walls.
            damageValue = 6000;
            defencePenetrationCoef = 0.5;
        }

        protected override void PrepareProtoObjectExplosive(out ExplosionPreset explosionPresets)
        {
            explosionPresets = ExplosionPresets.ExtraLargePragmiumResonanceBomb;
        }

        protected override void ServerExecuteExplosion(
            Vector2D positionEpicenter,
            IPhysicsSpace physicsSpace,
            WeaponFinalCache weaponFinalCache)
        {
            WeaponExplosionSystem.ServerProcessExplosionBomberman(
                positionEpicenter: positionEpicenter,
                physicsSpace: physicsSpace,
                damageDistanceFullDamage: DamageRadiusFullDamage,
                damageDistanceMax: DamageRadiusMax,
                damageDistanceDynamicObjectsOnly: DamageRadiusDynamicObjectsOnly,
                weaponFinalCache: weaponFinalCache,
                callbackCalculateDamageCoefByDistanceForStaticObjects:
                this.ServerCalculateDamageCoefByDistance,
                callbackCalculateDamageCoefByDistanceForDynamicObjects:
                ServerCalculateDamageCoefByDistanceForDynamicObjects);
        }

        private static double ServerCalculateDamageCoefByDistanceForDynamicObjects(double distance)
        {
            distance -= 1.42;
            distance = Math.Max(0, distance);
            return 1 - (distance / DamageRadiusDynamicObjectsOnly);
        }
    }
}