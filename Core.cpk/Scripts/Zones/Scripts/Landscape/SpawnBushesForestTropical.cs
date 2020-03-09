namespace AtomicTorch.CBND.CoreMod.Zones
{
    using System;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Bushes;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.SmallGatherables;
    using AtomicTorch.CBND.CoreMod.Triggers;

    public class SpawnBushesForestTropical : ProtoZoneSpawnScript
    {
        protected override void PrepareZoneSpawnScript(Triggers triggers, SpawnList spawnList)
        {
            triggers
                // trigger on world init
                .Add(GetTrigger<TriggerWorldInit>())
                // trigger on time interval
                .Add(GetTrigger<TriggerTimeInterval>().ConfigureForSpawn(TimeSpan.FromMinutes(15)));

            // bushes
            var bushesBerry = spawnList.CreatePreset(interval: 13, padding: 1)
                                       .Add<ObjectBushYellow>()
                                       .Add<ObjectBushPurple>()
                                       .Add<ObjectBushCoffee>()
                                       .SetCustomPaddingWithSelf(5);

            var bushWaterbulb = spawnList.CreatePreset(interval: 10, padding: 2)
                                         .Add<ObjectBushWaterbulb>()
                                         .SetCustomPaddingWithSelf(5)
                                         .SetCustomPaddingWith(bushesBerry, 5);

            var bushOilpod = spawnList.CreatePreset(interval: 25, padding: 2)
                                      .Add<ObjectBushOilpod>()
                                      .SetCustomPaddingWithSelf(5)
                                      .SetCustomPaddingWith(bushesBerry,   5)
                                      .SetCustomPaddingWith(bushWaterbulb, 5);

            var objectPineapple = spawnList.CreatePreset(interval: 16, padding: 1)
                                           .Add<ObjectSmallPineapple>()
                                           .SetCustomPaddingWithSelf(5)
                                           .SetCustomPaddingWith(bushesBerry,   5)
                                           .SetCustomPaddingWith(bushWaterbulb, 5)
                                           .SetCustomPaddingWith(bushOilpod,    5);
        }
    }
}