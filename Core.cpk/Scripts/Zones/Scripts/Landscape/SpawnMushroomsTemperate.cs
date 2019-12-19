namespace AtomicTorch.CBND.CoreMod.Zones
{
    using System;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.SmallGatherables;
    using AtomicTorch.CBND.CoreMod.Triggers;

    public class SpawnMushroomsTemperate : ProtoZoneSpawnScript
    {
        protected override void PrepareZoneSpawnScript(Triggers triggers, SpawnList spawnList)
        {
            triggers
                // trigger on world init
                .Add(GetTrigger<TriggerWorldInit>())
                // trigger on time interval
                .Add(GetTrigger<TriggerTimeInterval>().ConfigureForSpawn(TimeSpan.FromMinutes(10)));

            // mushrooms
            spawnList.CreatePreset(interval: 12, padding: 2)
                     .Add<ObjectSmallMushroomPennyBun>()
                     .Add<ObjectSmallMushroomRust>();
        }
    }
}