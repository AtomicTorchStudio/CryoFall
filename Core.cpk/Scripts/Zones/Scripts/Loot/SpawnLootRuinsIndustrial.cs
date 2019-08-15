namespace AtomicTorch.CBND.CoreMod.Zones
{
    using System;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Loot;
    using AtomicTorch.CBND.CoreMod.Triggers;

    public class SpawnLootRuinsIndustrial : ProtoZoneSpawnScript
    {
        public override bool CanSpawnIfPlayersNearby => true;

        protected override void PrepareZoneSpawnScript(Triggers triggers, SpawnList spawnList)
        {
            triggers
                // trigger on world init
                .Add(GetTrigger<TriggerWorldInit>())
                // trigger on time interval
                .Add(GetTrigger<TriggerTimeInterval>().Configure(TimeSpan.FromMinutes(30)));

            spawnList.CreatePreset(interval: 6, padding: 1)
                     .Add<ObjectLootPileGarbageLarge>(weight: 4)
                     .Add<ObjectLootCrateIndustrial>(weight: 3)
                     .Add<ObjectLootCrateSupply>(weight: 2)
                     .SetCustomPaddingWithSelf(9);
        }
    }
}