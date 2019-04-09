namespace AtomicTorch.CBND.CoreMod.Zones.Special
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters.Mobs;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Minerals;
    using AtomicTorch.CBND.CoreMod.Triggers;

    public class SpawnPragmiumWasteland : ProtoZoneSpawnScript
    {
        protected override void PrepareZoneSpawnScript(Triggers triggers, SpawnList spawnList)
        {
            triggers
                // trigger on world init
                .Add(GetTrigger<TriggerWorldInit>())
                // trigger on time interval
                .Add(GetTrigger<TriggerTimeInterval>().Configure(TimeSpan.FromMinutes(30)));

            spawnList.CreatePreset(interval: 10, padding: 2, useSectorDensity: false)
                     .Add<ObjectMineralPragmiumNode>();

            // mob spawn
            var presetLizard = spawnList.CreatePreset(interval: 22, padding: 0.5, useSectorDensity: false)
                                        .Add<MobCloakedLizard>()
                                        .SetCustomPaddingWithSelf(12);

            var presetScorpion = spawnList.CreatePreset(interval: 26, padding: 0.5, useSectorDensity: false)
                                          .Add<MobScorpion>()
                                          .SetCustomPaddingWithSelf(15);

            var presetCrawler = spawnList.CreatePreset(interval: 13, padding: 0.5, useSectorDensity: false)
                                         .Add<MobCrawler>()
                                         .SetCustomPaddingWithSelf(1);

            // define custom spawn padding between different mobs
            presetLizard.SetCustomPaddingWith(presetScorpion, 5);
            presetLizard.SetCustomPaddingWith(presetCrawler,  5);
            presetCrawler.SetCustomPaddingWith(presetScorpion, 5);
        }
    }
}