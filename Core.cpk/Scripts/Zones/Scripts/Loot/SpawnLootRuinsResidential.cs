namespace AtomicTorch.CBND.CoreMod.Zones
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Loot;
    using AtomicTorch.CBND.CoreMod.Triggers;

    public class SpawnLootRuinsResidential : ProtoZoneSpawnScript
    {
        public override bool CanSpawnIfPlayersNearby => true;

        protected override void PrepareZoneSpawnScript(Triggers triggers, SpawnList spawnList)
        {
            triggers
                // trigger on world init
                .Add(GetTrigger<TriggerWorldInit>())
                // trigger on time interval
                .Add(GetTrigger<TriggerTimeInterval>().ConfigureForSpawn(SpawnRuinsConstants.SpawnInterval));

            spawnList.CreatePreset(interval: 6, padding: 1)
                     .Add<ObjectLootPileGarbageLarge>(weight: 3)
                     .Add<ObjectLootCrateSupply>(weight: 1)
                     .Add<ObjectLootCrateFood>(weight: 1)
                     .SetCustomPaddingWithSelf(9);
        }
    }
}