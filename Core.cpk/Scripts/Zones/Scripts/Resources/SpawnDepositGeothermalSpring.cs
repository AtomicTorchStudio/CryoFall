namespace AtomicTorch.CBND.CoreMod.Zones
{
    using System;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Deposits;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SpawnDepositGeothermalSpring : ProtoZoneSpawnScript
    {
        // because this script called very rare we're increasing the spawn attempts count
        protected override double MaxSpawnAttempsMultiplier => 5;

        protected override void PrepareZoneSpawnScript(Triggers triggers, SpawnList spawnList)
        {
            triggers
                // trigger on world init
                .Add(GetTrigger<TriggerWorldInit>())
                // trigger on time interval
                .Add(GetTrigger<TriggerTimeInterval>()
                         .Configure(
                             intervalFrom: TimeSpan.FromHours(1),
                             intervalTo: TimeSpan.FromHours(2)));

            var presetGeothermalSpring = spawnList.CreatePreset(interval: 220, padding: 1);
            presetGeothermalSpring.AddExact<ObjectDepositGeothermalSpring>()
                                  .SetCustomPaddingWithSelf(75)
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

            // Let's ensure that we don't spawn geothermal spring too close to players' buildings.
            // take size of the largest land claim area
            var paddingToLandClaimsSize = Api.GetProtoEntity<ObjectLandClaimT4>().LandClaimWithGraceAreaSize;
            // add few extra tiles (as the objects are not 1*1 tile)
            paddingToLandClaimsSize += 6;

            presetGeothermalSpring.SetCustomPaddingWith(restrictionPresetLandclaim, paddingToLandClaimsSize);
        }
    }
}