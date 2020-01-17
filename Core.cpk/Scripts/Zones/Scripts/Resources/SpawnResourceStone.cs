namespace AtomicTorch.CBND.CoreMod.Zones
{
    using System;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Minerals;
    using AtomicTorch.CBND.CoreMod.Triggers;

    public class SpawnResourceStone : ProtoZoneSpawnScript
    {
        protected override void PrepareZoneSpawnScript(Triggers triggers, SpawnList spawnList)
        {
            triggers
                // trigger on world init
                .Add(GetTrigger<TriggerWorldInit>())
                // trigger on time interval
                .Add(GetTrigger<TriggerTimeInterval>().ConfigureForSpawn(TimeSpan.FromMinutes(15)));

            spawnList.CreatePreset(interval: 10, padding: 3)
                     .Add<ObjectMineralStone>();
        }
    }
}