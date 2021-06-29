﻿namespace AtomicTorch.CBND.CoreMod.Zones
{
    using System;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.SmallGatherables;
    using AtomicTorch.CBND.CoreMod.Triggers;

    public class SpawnMushroomsBoreal : ProtoZoneSpawnScript
    {
        protected override void PrepareZoneSpawnScript(Triggers triggers, SpawnList spawnList)
        {
            triggers
                // trigger on world init
                .Add(GetTrigger<TriggerWorldInit>())
                // trigger on time interval
                .Add(GetTrigger<TriggerTimeInterval>().ConfigureForSpawn(TimeSpan.FromMinutes(20)));

            // mushrooms
            spawnList.CreatePreset(interval: 16, padding: 2.0)
                     .Add<ObjectSmallMushroomPink>();

            spawnList.CreatePreset(interval: 16, padding: 1.5)
                     .Add<ObjectSmallMushroomRust>();
        }
    }
}