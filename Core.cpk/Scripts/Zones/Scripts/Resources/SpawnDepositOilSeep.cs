namespace AtomicTorch.CBND.CoreMod.Zones
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters.Mobs;
    using AtomicTorch.CBND.CoreMod.Helpers.Server;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Deposits;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Minerals;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Props.Road;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Special;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Data.Zones;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SpawnDepositOilSeep : ProtoZoneSpawnScript
    {
        // because this script called very rare we're increasing the spawn attempts count
        protected override double MaxSpawnAttempsMultiplier => 300;

        protected override void PrepareZoneSpawnScript(Triggers triggers, SpawnList spawnList)
        {
            // this resource is not spawned on the world init
            triggers
                // trigger on time interval
                .Add(GetTrigger<TriggerTimeInterval>()
                         .Configure(
                             intervalFrom: TimeSpan.FromHours(0.5),
                             intervalTo: TimeSpan.FromHours(1)));

            var restrictionPresetPragmium = spawnList.CreateRestrictedPreset()
                                                     .Add<ObjectMineralPragmiumSource>();

            var restrictionCharredGroundDeposit = spawnList.CreateRestrictedPreset()
                                                           .Add<ObjectCharredGround3Deposit>();

            var restrictionInfiniteOilSeep = spawnList.CreateRestrictedPreset()
                                                      .Add<ObjectDepositOilSeepInfinite>();

            var presetOilSeep = spawnList.CreatePreset(interval: 130, padding: 1, useSectorDensity: false);
            presetOilSeep.SpawnLimitPerIteration = 2;
            presetOilSeep.AddExact<ObjectDepositOilSeep>()
                         .SetCustomPaddingWithSelf(79)
                         .SetCustomPaddingWith(restrictionInfiniteOilSeep,      59)
                         .SetCustomPaddingWith(restrictionCharredGroundDeposit, 49)
                         // ensure no spawn near Pragmium
                         .SetCustomPaddingWith(restrictionPresetPragmium,
                                               SpawnResourcePragmium.PaddingPragmiumWithOilDeposit)
                         // ensure no spawn near cliffs
                         .SetCustomCanSpawnCheckCallback(
                             (physicsSpace, position)
                                 => ServerCheckAnyTileCollisions(physicsSpace,
                                                                 // offset on object layout center
                                                                 new Vector2D(position.X + 1.5,
                                                                              position.Y + 1.5),
                                                                 radius: 7));

            // don't spawn close to roads
            var restrictionPresetRoads = spawnList.CreateRestrictedPreset()
                                                  .Add<ObjectPropRoadHorizontal>()
                                                  .Add<ObjectPropRoadVertical>();
            presetOilSeep.SetCustomPaddingWith(restrictionPresetRoads, 20);

            // special restriction preset for player land claims
            var restrictionPresetLandclaim = spawnList.CreateRestrictedPreset()
                                                      .Add<IProtoObjectLandClaim>();

            // Let's ensure that we don't spawn oil seep too close to players' buildings.
            // take size of the largest land claim area
            var paddingToLandClaimsSize = (int)LandClaimSystem.MaxLandClaimSizeWithGraceArea.Value;
            // add few extra tiles (as the objects are not 1*1 tile)
            paddingToLandClaimsSize += 6;

            presetOilSeep.SetCustomPaddingWith(restrictionPresetLandclaim, paddingToLandClaimsSize);
        }

        protected override void ServerOnObjectSpawned(IGameObjectWithProto spawnedObject)
        {
            // spawn some guardian mobs so it will be harder to claim this deposit
            var objectOilSeep = (IStaticWorldObject)spawnedObject;
            ServerMobSpawnHelper.ServerTrySpawnMobsCustom(
                protoMob: Api.GetProtoEntity<MobScorpion>(),
                countToSpawn: 2,
                excludeBounds: objectOilSeep.Bounds.Inflate(1),
                maxSpawnDistanceFromExcludeBounds: 2,
                noObstaclesCheckRadius: 1,
                maxAttempts: 200);
        }

        protected override int SharedCalculatePresetDesiredCount(
            ObjectSpawnPreset preset,
            IServerZone zone,
            int currentCount,
            int desiredCountByDensity)
        {
            if (Api.IsEditor)
            {
                return desiredCountByDensity;
            }

            var hoursSinceWorldCreation = (Api.Server.Game.FrameTime - Api.Server.Game.WorldCreationTime)
                                          / (60 * 60);

            if (hoursSinceWorldCreation >= 48)
            {
                return desiredCountByDensity;
            }

            if (zone.ProtoGameObject is ZoneTemperateSwamp)
            {
                // to ensure the oil seeps in the swamp are balanced in A25
                // make their quantity 4 (and spawn them in pairs, seel later)
                desiredCountByDensity = (int)Math.Round(desiredCountByDensity * 1.33);
            }

            // throttle spawn to ensure even distribution of spawned objects during 48 hours since the startup
            var result = (int)(desiredCountByDensity * hoursSinceWorldCreation / 48);

            // to balance the oil spawn in A25
            // ensure that the initial oil seeps are spawned in pairs to make them harder to capture by a single team
            // e.g. rise the limit always by two
            // this also applies to the swamp
            result = (result / 2) * 2;

            return result;
        }
    }
}