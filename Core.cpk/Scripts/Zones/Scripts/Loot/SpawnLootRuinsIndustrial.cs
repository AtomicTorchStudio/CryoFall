namespace AtomicTorch.CBND.CoreMod.Zones
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Loot;
    using AtomicTorch.CBND.CoreMod.Triggers;

    public class SpawnLootRuinsIndustrial : ProtoZoneSpawnScript
    {
        protected override double MaxSpawnAttemptsMultiplier => 10;

        protected override void PrepareZoneSpawnScript(Triggers triggers, SpawnList spawnList)
        {
            triggers
                // trigger on world init
                .Add(GetTrigger<TriggerWorldInit>())
                // trigger on time interval
                .Add(GetTrigger<TriggerTimeInterval>().ConfigureForSpawn(SpawnRuinsConstants.SpawnInterval));

            var presetCrates = spawnList.CreatePreset(interval: 8, padding: 1, spawnAtLeastOnePerSector: true)
                                        .Add<ObjectLootCrateIndustrial>(weight: 3)
                                        .Add<ObjectLootCrateSupply>(weight: 2)
                                        .SetCustomPaddingWithSelf(9);
        }
    }
}