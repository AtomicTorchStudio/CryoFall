namespace AtomicTorch.CBND.CoreMod.Zones
{
    using System;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.SmallGatherables;
    using AtomicTorch.CBND.CoreMod.Triggers;

    public class SpawnResourceHerbs : ProtoZoneSpawnScript
    {
        protected override void PrepareZoneSpawnScript(Triggers triggers, SpawnList spawnList)
        {
            triggers
                // trigger on world init
                .Add(GetTrigger<TriggerWorldInit>())
                // trigger on time interval
                .Add(GetTrigger<TriggerTimeInterval>().ConfigureForSpawn(TimeSpan.FromMinutes(20)));

            var greenHerbs = spawnList.CreatePreset(interval: 12, padding: 2)
                                      .Add<ObjectSmallHerbGreen>()
                                      .SetCustomPaddingWithSelf(5);

            var redHerbs = spawnList.CreatePreset(interval: 18, padding: 2)
                                    .Add<ObjectSmallHerbRed>()
                                    .SetCustomPaddingWithSelf(5)
                                    .SetCustomPaddingWith(greenHerbs, 5);

            spawnList.CreatePreset(interval: 25, padding: 2)
                     .Add<ObjectSmallHerbPurple>()
                     .SetCustomPaddingWithSelf(10)
                     .SetCustomPaddingWith(greenHerbs, 10)
                     .SetCustomPaddingWith(redHerbs,   10);
        }
    }
}