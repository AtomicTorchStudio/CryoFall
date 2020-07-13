namespace AtomicTorch.CBND.CoreMod.Zones
{
    using System;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Loot;
    using AtomicTorch.CBND.CoreMod.Triggers;

    public class SpawnLootRoads : ProtoZoneSpawnScript
    {
        protected override void PrepareZoneSpawnScript(Triggers triggers, SpawnList spawnList)
        {
            triggers
                .Add(GetTrigger<TriggerWorldInit>())
                .Add(GetTrigger<TriggerTimeInterval>().ConfigureForSpawn(TimeSpan.FromMinutes(15)));

            spawnList.CreatePreset(interval: 14, padding: 1)
                     .Add<ObjectLootPileGarbage1>(weight: 3)
                     .Add<ObjectLootPileGarbage2>(weight: 3)
                     .Add<ObjectLootPileGarbage3>(weight: 1)
                     .SetCustomPaddingWithSelf(14);
        }
    }
}