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
                // (please note when changing this value to adjust the destruction timeout in ObjectMineralPragmiumNode)
                .Add(GetTrigger<TriggerTimeInterval>().Configure(TimeSpan.FromMinutes(30)));

            spawnList.CreatePreset(interval: 10, padding: 2, useSectorDensity: false)
                     .Add<ObjectMineralPragmiumNode>();

            // mob spawn
            var presetLizard = spawnList.CreatePreset(interval: 22, padding: 0.5, useSectorDensity: false)
                                        .Add<MobFireLizard>()
                                        .SetCustomPaddingWithSelf(12);

            var presetScorpion = spawnList.CreatePreset(interval: 26, padding: 0.5, useSectorDensity: false)
                                          .Add<MobScorpion>()
                                          .SetCustomPaddingWithSelf(15);

            var presetBeetle = spawnList.CreatePreset(interval: 15, padding: 0.5, useSectorDensity: false)
                                         .Add<MobPragmiumBeetle>()
                                         .SetCustomPaddingWithSelf(5);

            // define custom spawn padding between different mobs
            presetLizard.SetCustomPaddingWith(presetScorpion, 5);
            presetLizard.SetCustomPaddingWith(presetBeetle,  5);
            presetBeetle.SetCustomPaddingWith(presetScorpion, 5);
        }
    }
}