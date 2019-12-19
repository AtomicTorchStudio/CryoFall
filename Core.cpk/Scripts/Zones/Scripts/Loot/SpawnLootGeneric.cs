namespace AtomicTorch.CBND.CoreMod.Zones
{
    using System;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Loot;
    using AtomicTorch.CBND.CoreMod.Triggers;

    public class SpawnLootGeneric : ProtoZoneSpawnScript
    {
        protected override void PrepareZoneSpawnScript(Triggers triggers, SpawnList spawnList)
        {
            triggers
                // trigger on world init
                .Add(GetTrigger<TriggerWorldInit>())
                // trigger on time interval
                .Add(GetTrigger<TriggerTimeInterval>().ConfigureForSpawn(TimeSpan.FromMinutes(5)));

            // grass
            spawnList.CreatePreset(interval: 7, padding: 1)
                     .Add<ObjectLootGrass>()
                     .SetCustomPaddingWithSelf(2);

            // twigs
            spawnList.CreatePreset(interval: 7, padding: 1)
                     .Add<ObjectLootTwigs>()
                     .SetCustomPaddingWithSelf(4);

            // stones
            spawnList.CreatePreset(interval: 6, padding: 1)
                     .Add<ObjectLootStone>()
                     .SetCustomPaddingWithSelf(4);
        }
    }
}