namespace AtomicTorch.CBND.CoreMod.Zones
{
    using System;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Minerals;
    using AtomicTorch.CBND.CoreMod.Triggers;

    public class SpawnResourcesGeneric : ProtoZoneSpawnScript
    {
        protected override void PrepareZoneSpawnScript(Triggers triggers, SpawnList spawnList)
        {
            triggers
                // trigger on world init
                .Add(GetTrigger<TriggerWorldInit>())
                // trigger on time interval
                .Add(GetTrigger<TriggerTimeInterval>().ConfigureForSpawn(TimeSpan.FromMinutes(5)));

            // all basic mineral node types
            // excludes things like coal, saltpeter, sulfur and other advanced minerals
            spawnList.CreatePreset(interval: 10, padding: 3)
                     .Add<ObjectMineralCopper>()
                     .Add<ObjectMineralIron>()
                     .Add<ObjectMineralStone>()
                     .Add<ObjectMineralSand>()
                     .Add<ObjectMineralClay>();
        }
    }
}