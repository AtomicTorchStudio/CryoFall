namespace AtomicTorch.CBND.CoreMod.Characters.Mobs
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
    using AtomicTorch.CBND.CoreMod.Helpers.Server;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Explosives;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.CBND.GameApi.Extensions;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class MobBroodNest
        : ProtoCharacterMob
            <MobBroodNest.PrivateState,
                CharacterMobPublicState,
                CharacterMobClientState>
    {
        private const double ExplosionWorldOffsetY = 0.5;

        private const int MobDespawnDistance = 10;

        // How many drone mobs it can have simultaneously
        private const int MobsCountLimit = 3;

        private const int MobSpawnDistance = 1;

        // How often it will attempt to (re)spawn drones
        private const int ServerSpawnDronesIntervalSeconds = 12;

        private const int ServerSpawnMobsMaxCountPerIteration = 1; // (re)spawn 1 drone per iteration

        private static readonly Lazy<IProtoCharacterMob> ProtoCharacterDrone1Lazy
            = new(GetProtoEntity<MobBroodDrone>);

        private static readonly Lazy<IProtoCharacterMob> ProtoCharacterDrone2Lazy
            = new(GetProtoEntity<MobBroodGuardian>);

        public override bool AiIsRunAwayFromHeavyVehicles => false;

        public override double AutoAggroNotificationDistance => 0;

        public override float CharacterWorldHeight => 2.0f;

        public override double CorpseInteractionAreaScale => 1.1;

        public override double MobKillExperienceMultiplier => 1.0;

        public override string Name => "Brood nest";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.HardTissues;

        public override double StatDefaultHealthMax => 1000;

        public override double StatMoveSpeed => 0;

        public float VolumeExplosion => 1;

        private static ExplosionPreset ExplosionPreset => ExplosionPresets.KeiniteBroodNest;

        public override void ServerOnDestroy(ICharacter gameObject)
        {
            base.ServerOnDestroy(gameObject);

            using var observers = Api.Shared.GetTempList<ICharacter>();

            // this event is propagated on a larger distance and has sound cues
            Server.World.GetCharactersInRadius(gameObject.TilePosition,
                                               observers,
                                               radius: this.SoundEventsNetworkRadius,
                                               onlyPlayers: true);
            this.CallClient(observers.AsList(),
                            _ => _.ClientRemote_OnKilled(gameObject.Position));

            var damageRadius = 2.1;
            var DestroyedExplosionPreset = ExplosionPreset;

            var damageDescriptionCharacters = new DamageDescription(
                damageValue: 75,
                armorPiercingCoef: 0.35,
                finalDamageMultiplier: 1,
                rangeMax: damageRadius,
                damageDistribution: new DamageDistribution(DamageType.Kinetic, 1));

            SharedExplosionHelper.ServerExplode(
                character: gameObject,
                protoExplosive: null,
                protoWeapon: null,
                explosionPreset: DestroyedExplosionPreset,
                epicenterPosition: gameObject.Position,
                damageDescriptionCharacters: damageDescriptionCharacters,
                physicsSpace: gameObject.PhysicsBody.PhysicsSpace,
                executeExplosionCallback: ServerExecuteVehicleExplosion);

            // spawn mobs immediately after the explosion
            ServerTimersSystem.AddAction(0.3,
                                         () => ServerTrySpawnMobs(gameObject, applySpawnPerIterationLimit: false));

            void ServerExecuteVehicleExplosion(
                Vector2D positionEpicenter,
                IPhysicsSpace physicsSpace,
                WeaponFinalCache weaponFinalCache)
            {
                WeaponExplosionSystem.ServerProcessExplosionCircle(
                    positionEpicenter: positionEpicenter,
                    physicsSpace: physicsSpace,
                    damageDistanceMax: damageRadius,
                    weaponFinalCache: weaponFinalCache,
                    damageOnlyDynamicObjects: false,
                    isDamageThroughObstacles: false,
                    callbackCalculateDamageCoefByDistanceForStaticObjects: CalcDamageCoefByDistance,
                    callbackCalculateDamageCoefByDistanceForDynamicObjects: CalcDamageCoefByDistance);

                double CalcDamageCoefByDistance(double distance)
                {
                    var distanceThreshold = 0.5;
                    if (distance <= distanceThreshold)
                    {
                        return 1;
                    }

                    distance -= distanceThreshold;
                    distance = Math.Max(0, distance);

                    var maxDistance = damageRadius;
                    maxDistance -= distanceThreshold;
                    maxDistance = Math.Max(0, maxDistance);

                    return 1 - Math.Min(distance / maxDistance, 1);
                }
            }
        }

        protected override void FillDefaultEffects(Effects effects)
        {
            base.FillDefaultEffects(effects);

            effects
                .AddValue(this, StatName.DefenseKinetic,  0.3)
                .AddValue(this, StatName.DefenseHeat,     0.3)
                .AddValue(this, StatName.DefenseChemical, 1.0);
        }

        protected override void PrepareProtoCharacterMob(
            out ProtoCharacterSkeleton skeleton,
            ref double scale,
            DropItemsList lootDroplist)
        {
            skeleton = GetProtoEntity<SkeletonBroodNest>();

            // primary loot
            lootDroplist
                .Add<ItemKeinite>(count: 5,       countRandom: 3)
                .Add<ItemInsectMeatRaw>(count: 5, countRandom: 3)
                .Add<ItemSlime>(count: 2,         countRandom: 1);

            // extra loot
            lootDroplist.Add(condition: SkillHunting.ServerRollExtraLoot,
                             nestedList: new DropItemsList(outputs: 1)
                                         .Add<ItemInsectMeatRaw>(count: 1)
                                         .Add<ItemSlime>(count: 1));
        }

        protected override void ServerInitializeCharacterMob(ServerInitializeData data)
        {
            base.ServerInitializeCharacterMob(data);

            var characterNest = data.GameObject;
            if (data.IsFirstTimeInit)
            {
                ServerTimersSystem.AddAction(
                    0.1,
                    () =>
                    {
                        if (characterNest.IsDestroyed)
                        {
                            return;
                        }

                        ServerTrySpawnMobs(characterNest, applySpawnPerIterationLimit: false);
                    });
            }

            TickSpawn(characterNest);

            static void TickSpawn(ICharacter characterNest)
            {
                if (characterNest.IsDestroyed)
                {
                    return;
                }

                // schedule update again
                ServerTimersSystem.AddAction(
                    ServerSpawnDronesIntervalSeconds,
                    () => TickSpawn(characterNest));

                ServerTrySpawnMobs(characterNest, applySpawnPerIterationLimit: true);
            }
        }

        protected override void ServerOnAggro(ICharacter characterMob, ICharacter characterToAggro)
        {
            // this mob is passive
        }

        protected override void ServerUpdateMob(ServerUpdateData data)
        {
            // it's a static mob
        }

        private static void ServerTrySpawnMobs(ICharacter characterNest, bool applySpawnPerIterationLimit)
        {
            if (LandClaimSystem.SharedIsLandClaimedByAnyone(characterNest.TilePosition))
            {
                // don't spawn mobs as the land is claimed
                return;
            }

            // calculate how many creatures are still alive
            var mobsList = GetPrivateState(characterNest).MobsList;

            var mobsAlive = 0;
            for (var index = 0; index < mobsList.Count; index++)
            {
                var character = mobsList[index];
                if (character.IsDestroyed)
                {
                    mobsList.RemoveAt(index--);
                    continue;
                }

                if (character.TilePosition.TileSqrDistanceTo(characterNest.TilePosition)
                    > MobDespawnDistance * MobDespawnDistance)
                {
                    // the drone mob is too far - probably lured away by a player
                    using var tempListObservers = Api.Shared.GetTempList<ICharacter>();
                    Server.World.GetScopedByPlayers(character, tempListObservers);
                    if (tempListObservers.Count == 0)
                    {
                        // despawn this mob as it's not observed by any player
                        Server.World.DestroyObject(character);
                        mobsList.RemoveAt(index--);
                    }

                    continue;
                }

                mobsAlive++;
            }

            var countToSpawn = MobsCountLimit - mobsAlive;
            if (countToSpawn <= 0)
            {
                return;
            }

            // spawn mobs(s) nearby
            if (applySpawnPerIterationLimit)
            {
                countToSpawn = Math.Min(countToSpawn, ServerSpawnMobsMaxCountPerIteration);
            }

            for (var index = 0; index < countToSpawn; index++)
            {
                ServerMobSpawnHelper.ServerTrySpawnMobsCustom(
                    protoMob: RandomHelper.Next(2) == 0
                                  ? ProtoCharacterDrone1Lazy.Value
                                  : ProtoCharacterDrone2Lazy.Value,
                    spawnedCollection: mobsList,
                    countToSpawn: 1,
                    excludeBounds: new RectangleInt(characterNest.TilePosition, (1, 1)),
                    maxSpawnDistanceFromExcludeBounds: MobSpawnDistance,
                    noObstaclesCheckRadius: 0.5,
                    maxAttempts: 50);
            }
        }

        private void ClientRemote_OnKilled(Vector2D worldPosition)
        {
            SharedExplosionHelper.ClientExplode(worldPosition + (0, ExplosionWorldOffsetY),
                                                ExplosionPresets.KeiniteBroodNest,
                                                this.VolumeExplosion);
        }

        public class PrivateState : CharacterMobPrivateState
        {
            [TempOnly]
            public List<ICharacter> MobsList { get; } = new();
        }
    }
}