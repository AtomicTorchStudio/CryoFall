namespace AtomicTorch.CBND.CoreMod.Zones
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters.Mobs;
    using AtomicTorch.CBND.CoreMod.Triggers;

    public class SpawnMobsVolcanic : ProtoZoneSpawnScript
    {
        protected override void PrepareZoneSpawnScript(Triggers triggers, SpawnList spawnList)
        {
            triggers
                .Add(GetTrigger<TriggerWorldInit>())
                .Add(GetTrigger<TriggerTimeInterval>().ConfigureForSpawn(TimeSpan.FromMinutes(15)));

            var crawler = spawnList.CreatePreset(interval: 12, padding: 0.5)
                                   .Add<MobCrawler>()
                                   .SetCustomPaddingWithSelf(12);

            var fireLizard = spawnList.CreatePreset(interval: 15, padding: 0.5)
                                      .Add<MobFireLizard>()
                                      .SetCustomPaddingWith(crawler, 15)
                                      .SetCustomPaddingWithSelf(15);

            var blackBeetle = spawnList.CreatePreset(interval: 15, padding: 0.5)
                                       .Add<MobBlackBeetle>()
                                       .SetCustomPaddingWith(crawler,    15)
                                       .SetCustomPaddingWith(fireLizard, 15)
                                       .SetCustomPaddingWithSelf(15);
        }
    }
}