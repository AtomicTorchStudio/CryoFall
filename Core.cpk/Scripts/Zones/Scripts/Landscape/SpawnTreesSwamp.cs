namespace AtomicTorch.CBND.CoreMod.Zones
{
    using System;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Trees;
    using AtomicTorch.CBND.CoreMod.Triggers;

    public class SpawnTreesSwamp : ProtoZoneSpawnScript
    {
        protected override void PrepareZoneSpawnScript(Triggers triggers, SpawnList spawnList)
        {
            triggers
                // trigger on world init
                .Add(GetTrigger<TriggerWorldInit>())
                // trigger on time interval
                .Add(GetTrigger<TriggerTimeInterval>().ConfigureForSpawn(TimeSpan.FromMinutes(10)));

            // all trees
            spawnList.CreatePreset(interval: 3.5, padding: 1.5)
                     .Add<ObjectTreeSwamp>()
                     .Add<ObjectTreeWillow>()
                     .Add<ObjectTreeDeadMossy1>(weight: 1.0 / 5.0)
                     .Add<ObjectTreeDeadMossy2>(weight: 1.0 / 5.0)
                     .Add<ObjectTreeStumpMossy>(weight: 1.0 / 5.0)
                     .Add<ObjectTreeSnagMossy>(weight: 1.0 / 5.0)
                     .SetCustomPaddingWithSelf(2.1);
        }
    }
}