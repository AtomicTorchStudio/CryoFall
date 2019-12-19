namespace AtomicTorch.CBND.CoreMod.Zones
{
    using System;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Trees;
    using AtomicTorch.CBND.CoreMod.Triggers;

    public class SpawnStumpsTemperate : ProtoZoneSpawnScript
    {
        protected override void PrepareZoneSpawnScript(Triggers triggers, SpawnList spawnList)
        {
            triggers
                // trigger on world init
                .Add(GetTrigger<TriggerWorldInit>())
                // trigger on time interval
                .Add(GetTrigger<TriggerTimeInterval>().ConfigureForSpawn(TimeSpan.FromMinutes(10)));

            // stumps
            var stumps = spawnList.CreatePreset(interval: 20, padding: 1.5)
                                  .Add<ObjectTreeStump>()
                                  .Add<ObjectTreeStumpWithTwig>()
                                  .SetCustomPaddingWithSelf(15);

            // fallen tree
            var fallenTree = spawnList.CreatePreset(interval: 40, padding: 2)
                                      .Add<ObjectTreeFallen>()
                                      .SetCustomPaddingWithSelf(25)
                                      .SetCustomPaddingWith(stumps, 10);
        }
    }
}