namespace AtomicTorch.CBND.CoreMod.Zones
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters.Mobs;
    using AtomicTorch.CBND.CoreMod.Helpers.Server;
    using AtomicTorch.CBND.CoreMod.Rates;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Deposits;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Props.Road;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Special;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier3.XenoGeology;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Data.Zones;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SpawnDepositGeothermalSpring : ProtoZoneSpawnScript
    {
        private double spawnRateMultiplier;

        // because this script called very rarely we're increasing the spawn attempts count
        protected override double MaxSpawnAttemptsMultiplier => 300;

        protected override void PrepareZoneSpawnScript(Triggers triggers, SpawnList spawnList)
        {
            this.spawnRateMultiplier = RateResourcesPvPDepositLithiumMultiplier.SharedValue;

            // this resource is not spawned on the world init
            triggers
                // trigger on time interval
                .Add(GetTrigger<TriggerTimeInterval>()
                         .Configure(
                             intervalFrom: TimeSpan.FromHours(0.5),
                             intervalTo: TimeSpan.FromHours(1)));

            var restrictionInfiniteGeothermalSpring = spawnList.CreateRestrictedPreset()
                                                               .Add<ObjectDepositGeothermalSpringInfinite>();

            var restrictionDepletedDeposit = spawnList.CreateRestrictedPreset()
                                                      .Add<ObjectDepletedDeposit>();

            var presetGeothermalSpring = spawnList.CreatePreset(interval: 225, padding: 1, useSectorDensity: false);
            presetGeothermalSpring.SpawnLimitPerIteration = 1;
            presetGeothermalSpring.AddExact<ObjectDepositGeothermalSpring>()
                                  .SetCustomPaddingWithSelf(79)
                                  .SetCustomPaddingWith(restrictionInfiniteGeothermalSpring, 59)
                                  .SetCustomPaddingWith(restrictionDepletedDeposit,          49)
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
            presetGeothermalSpring.SetCustomPaddingWith(restrictionPresetRoads, 20);

            // special restriction preset for player land claims
            var restrictionPresetLandclaim = spawnList.CreateRestrictedPreset()
                                                      .Add<IProtoObjectLandClaim>();

            // Let's ensure that we don't spawn geothermal spring too close to players' buildings.
            // take size of the largest land claim area
            var paddingToLandClaimsSize = (int)LandClaimSystem.MaxLandClaimSizeWithGraceArea.Value;
            // add few extra tiles (as the objects are not 1*1 tile)
            paddingToLandClaimsSize += 6;

            presetGeothermalSpring.SetCustomPaddingWith(restrictionPresetLandclaim, paddingToLandClaimsSize);
        }

        protected override void ServerOnObjectSpawned(IGameObjectWithProto spawnedObject)
        {
            // spawn some guardian mobs so it will be harder to claim this deposit
            var objectGeothermalSpring = (IStaticWorldObject)spawnedObject;
            ServerMobSpawnHelper.ServerTrySpawnMobsCustom(
                protoMob: Api.GetProtoEntity<MobCloakedLizard>(),
                countToSpawn: 3,
                excludeBounds: objectGeothermalSpring.Bounds.Inflate(1),
                maxSpawnDistanceFromExcludeBounds: 2,
                noObstaclesCheckRadius: 0.5,
                maxAttempts: 200);
        }

        protected override int SharedCalculatePresetDesiredCount(
            ObjectSpawnPreset preset,
            IServerZone zone,
            int currentCount,
            int desiredCountByDensity)
        {
            // apply the server rate
            desiredCountByDensity = (int)Math.Round(desiredCountByDensity * this.spawnRateMultiplier,
                                                    MidpointRounding.AwayFromZero);

            if (zone.ProtoGameObject is ZoneBorealForest)
            {
                // to balance boreal and temperate forests and ensure they have a bit different number of resources for A26
                // we're increasing the spawn density in boreal forest
                desiredCountByDensity = (int)Math.Round(desiredCountByDensity * 1.25);
            }

            if (Api.IsEditor)
            {
                return desiredCountByDensity;
            }

            // throttle spawn to ensure even distribution of spawned objects during specified hours since the startup
            const double spawnSpreadDurationHours = 48;

            var hoursSinceWorldCreation = Api.Server.Game.SecondsSinceWorldCreation / (60 * 60);

            // apply the timegate offset as there should be no deposits spawn until Xenogeology is available for research
            var timeGateHours = Api.GetProtoEntity<TechGroupXenogeologyT3>().TimeGatePvP / (60 * 60);
            if (timeGateHours > 0)
            {
                // as there is a timegate ensure the spawn could start immediately after it's expired without requiring any additional time
                timeGateHours -= spawnSpreadDurationHours / desiredCountByDensity;
                timeGateHours = Math.Max(0, timeGateHours);
                hoursSinceWorldCreation -= timeGateHours;
                hoursSinceWorldCreation = Math.Max(0, hoursSinceWorldCreation);
            }

            if (hoursSinceWorldCreation >= spawnSpreadDurationHours)
            {
                return desiredCountByDensity;
            }

            // throttle spawn to ensure even distribution of spawned objects during 48 hours since the startup
            return (int)(desiredCountByDensity * hoursSinceWorldCreation / 48);
        }
    }
}