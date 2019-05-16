namespace AtomicTorch.CBND.CoreMod.Zones
{
    using System;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Deposits;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Explosives;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Minerals;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class SpawnResourcePragmium : ProtoZoneSpawnScript
    {
        public const int PaddingPragmiumWithOilDeposit = 75;

        // because this script called very rare we're increasing the spawn attempts count
        protected override double MaxSpawnAttempsMultiplier => 5;

        protected override void PrepareZoneSpawnScript(Triggers triggers, SpawnList spawnList)
        {
            triggers
                // trigger on world init
                .Add(GetTrigger<TriggerWorldInit>())
                // trigger on time interval
                .Add(GetTrigger<TriggerTimeInterval>().Configure(TimeSpan.FromHours(8)));

            var presetPragmiumSource = spawnList.CreatePreset(interval: 180, padding: 3)
                                                .Add<ObjectMineralPragmiumSource>()
                                                .SetCustomPaddingWithSelf(75);

            var restrictionPresetDepositOilSeep = spawnList.CreateRestrictedPreset()
                                                           .Add<ObjectDepositOilSeep>();

            presetPragmiumSource.SetCustomPaddingWith(restrictionPresetDepositOilSeep,
                                                      PaddingPragmiumWithOilDeposit);

            // special restriction preset for player land claims
            var restrictionPresetLandclaim = spawnList.CreateRestrictedPreset()
                                                      .Add<IProtoObjectLandClaim>();

            // Let's ensure that we don't spawn Pragmium Source too close to players' buildings.
            // take half size of the largest land claim area
            var paddingToLandClaimsSize = Api.GetProtoEntity<ObjectLandClaimT4>().LandClaimWithGraceAreaSize / 2.0;
            // add the explosion radius
            paddingToLandClaimsSize += Api.GetProtoEntity<ObjectMineralPragmiumSourceExplosion>()
                                          .DamageRadius;
            // add few extra tiles (as the objects are not 1*1 tile)
            paddingToLandClaimsSize += 6;

            presetPragmiumSource.SetCustomPaddingWith(restrictionPresetLandclaim, paddingToLandClaimsSize);
        }
    }
}