namespace AtomicTorch.CBND.CoreMod.StaticObjects.Explosives.Bombs
{
    using System;
    using AtomicTorch.CBND.CoreMod.Systems.ItemExplosive;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.CBND.GameApi.Scripting;
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

        public override bool IsActivatesRaidModeForLandClaim => true;

        public override string Name => "Resonance bomb";

        public override double ServerCalculateDamageCoefByDistanceForDynamicObjects(double distance)
        {
            var distanceThreshold = 1;
            if (distance <= distanceThreshold)
            {
                return 1;
            }

            distance -= distanceThreshold;
            distance = Math.Max(0, distance);

            var maxDistance = DamageRadiusDynamicObjectsOnly;
            maxDistance -= distanceThreshold;
            maxDistance = Math.Max(0, maxDistance);

            return 1 - Math.Min(distance / maxDistance, 1);
        }

        public override double ServerCalculateDamageCoefByDistanceForStaticObjects(double distance)
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

        protected override void ClientOnObjectDestroyed(Vector2D position)
        {
            //base.ClientOnObjectDestroyed(position);
            Logger.Important(this + " exploded at " + position);

            var explosionPresetNode = ExplosionPresets.PragmiumResonanceBomb_NodeClientOnly;
            var positionEpicenter = position + this.Layout.Center;
            ProcessExplosionDirection(-1, 0);  // left
            ProcessExplosionDirection(0,  1);  // top
            ProcessExplosionDirection(1,  0);  // right
            ProcessExplosionDirection(0,  -1); // bottom

            ExplosionHelper.ClientExplode(position: position + this.Layout.Center,
                                          this.ExplosionPreset,
                                          this.VolumeExplosion);

            void ProcessExplosionDirection(int xOffset, int yOffset)
            {
                foreach (var (_, offsetIndex) in
                    WeaponExplosionSystem.SharedEnumerateExplosionBombermanDirectionTilesWithTargets(
                        positionEpicenter: positionEpicenter,
                        damageDistanceFullDamage:
                        DamageRadiusFullDamage,
                        damageDistanceMax: DamageRadiusMax,
                        Api.Client.World,
                        xOffset,
                        yOffset))
                {
                    ClientTimersSystem.AddAction(
                        delaySeconds: 0.1 * offsetIndex, // please note the offsetIndex is starting with 1
                        () => ExplosionHelper.ClientExplode(
                            position: positionEpicenter + (offsetIndex * xOffset, offsetIndex * yOffset),
                            explosionPresetNode,
                            volume: 0));
                }
            }
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
            damageValue = 12000;
            defencePenetrationCoef = 0.5;
        }

        protected override void PrepareProtoObjectExplosive(out ExplosionPreset explosionPresets)
        {
            explosionPresets = ExplosionPresets.PragmiumResonanceBomb_Center;
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
                this.ServerCalculateDamageCoefByDistanceForStaticObjects,
                callbackCalculateDamageCoefByDistanceForDynamicObjects:
                this.ServerCalculateDamageCoefByDistanceForDynamicObjects);
        }
    }
}