namespace AtomicTorch.CBND.CoreMod.Zones
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters.Mobs;
    using AtomicTorch.CBND.CoreMod.Helpers.Server;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Deposits;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Minerals;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SpawnDepositOilSeep : ProtoZoneSpawnScript
    {
        // because this script called very rare we're increasing the spawn attempts count
        protected override double MaxSpawnAttempsMultiplier => 5;

        protected override void PrepareZoneSpawnScript(Triggers triggers, SpawnList spawnList)
        {
            // this resource is not spawned on the world init
            triggers
                // trigger on time interval
                .Add(GetTrigger<TriggerTimeInterval>()
                         .Configure(
                             intervalFrom: TimeSpan.FromHours(1),
                             intervalTo: TimeSpan.FromHours(2)));

            var restrictionPresetPragmium = spawnList.CreateRestrictedPreset()
                                                     .Add<ObjectMineralPragmiumSource>();

            var restrictionInfiniteOilSeep = spawnList.CreateRestrictedPreset()
                                                      .Add<ObjectDepositOilSeepInfinite>();

            var presetOilSeep = spawnList.CreatePreset(interval: 160, padding: 1);
            presetOilSeep.AddExact<ObjectDepositOilSeep>()
                         .SetCustomPaddingWithSelf(75)
                         .SetCustomPaddingWith(restrictionInfiniteOilSeep, 75)
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
    }
}