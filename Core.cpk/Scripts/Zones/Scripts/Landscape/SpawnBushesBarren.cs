namespace AtomicTorch.CBND.CoreMod.Zones
{
    using System;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Bushes;
    using AtomicTorch.CBND.CoreMod.Triggers;

    public class SpawnBushesBarren : ProtoZoneSpawnScript
    {
        protected override void PrepareZoneSpawnScript(Triggers triggers, SpawnList spawnList)
        {
            triggers
                // trigger on world init
                .Add(GetTrigger<TriggerWorldInit>())
                // trigger on time interval
                .Add(GetTrigger<TriggerTimeInterval>().Configure(TimeSpan.FromMinutes(10)));

            // bushes (barren landscape)
            spawnList.CreatePreset(interval: 20, padding: 2)
                     .Add<ObjectBushYucca>()
                     .SetCustomPaddingWithSelf(10);
        }
    }
}