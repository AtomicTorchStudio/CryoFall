namespace AtomicTorch.CBND.CoreMod.Zones
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters.Mobs;
    using AtomicTorch.CBND.CoreMod.Triggers;

    public class SpawnMobsRiverSnail : ProtoZoneSpawnScript
    {
        protected override void PrepareZoneSpawnScript(Triggers triggers, SpawnList spawnList)
        {
            triggers
                .Add(GetTrigger<TriggerWorldInit>())
                .Add(GetTrigger<TriggerTimeInterval>().Configure(TimeSpan.FromMinutes(5)));

            spawnList.CreatePreset(interval: 20, padding: 0.5, useSectorDensity: false)
                     .Add<MobRiverSnail>()
                     .SetCustomPaddingWithSelf(25);
        }
    }
}