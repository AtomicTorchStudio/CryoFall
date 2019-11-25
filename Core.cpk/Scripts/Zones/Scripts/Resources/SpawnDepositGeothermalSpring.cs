namespace AtomicTorch.CBND.CoreMod.Zones
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters.Mobs;
    using AtomicTorch.CBND.CoreMod.Helpers.Server;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Deposits;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Props.Road;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Special;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SpawnDepositGeothermalSpring : ProtoZoneSpawnScript
    {
        // because this script called very rare we're increasing the spawn attempts count
        protected override double MaxSpawnAttempsMultiplier => 50;

        protected override void PrepareZoneSpawnScript(Triggers triggers, SpawnList spawnList)
        {
            // this resource is not spawned on the world init
            triggers
                // trigger on time interval
                .Add(GetTrigger<TriggerTimeInterval>()
                         .Configure(
                             intervalFrom: TimeSpan.FromHours(1),
                             intervalTo: TimeSpan.FromHours(2)));

            var restrictionInfiniteGeothermalSpring = spawnList.CreateRestrictedPreset()
                                                               .Add<ObjectDepositGeothermalSpringInfinite>();

            var restrictionCharredGroundDeposit = spawnList.CreateRestrictedPreset()
                                                           .Add<ObjectCharredGround3Deposit>();

            var presetGeothermalSpring = spawnList.CreatePreset(interval: 175, padding: 1, useSectorDensity: false);
            presetGeothermalSpring.SpawnLimitPerIteration = 1;
            presetGeothermalSpring.AddExact<ObjectDepositGeothermalSpring>()
                                  .SetCustomPaddingWithSelf(79)
                                  .SetCustomPaddingWith(restrictionInfiniteGeothermalSpring, 79)
                                  .SetCustomPaddingWith(restrictionCharredGroundDeposit,     79)
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
    }
}