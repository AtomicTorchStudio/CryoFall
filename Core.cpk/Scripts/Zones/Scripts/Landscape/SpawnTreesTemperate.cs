namespace AtomicTorch.CBND.CoreMod.Zones
{
    using System;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Trees;
    using AtomicTorch.CBND.CoreMod.Triggers;

    public class SpawnTreesTemperate : ProtoZoneSpawnScript
    {
        protected override void PrepareZoneSpawnScript(Triggers triggers, SpawnList spawnList)
        {
            triggers
                // trigger on world init
                .Add(GetTrigger<TriggerWorldInit>())
                // trigger on time interval
                .Add(GetTrigger<TriggerTimeInterval>().Configure(TimeSpan.FromMinutes(10)));

            // regular trees
            var regularTrees = spawnList.CreatePreset(interval: 4, padding: 1.5)
                                        .Add<ObjectTreeBirch>()
                                        .Add<ObjectTreeOak>()
                                        .SetCustomPaddingWithSelf(2.1);

            // rubber trees
            var rubberTrees = spawnList.CreatePreset(interval: 20, padding: 1.5)
                                       .Add<ObjectTreeRubber>()
                                       .SetCustomPaddingWithSelf(5.0);

            // clusters of pines
            spawnList.CreatePreset(interval: 10, padding: 1.5, useSectorDensity: false)
                     .Add<ObjectTreePine>()
                     .SetCustomPaddingWith(regularTrees, 6)
                     .SetCustomPaddingWith(rubberTrees,  6)
                     // not directly near (left, right, etc.), but diagonally - ok!
                     .SetCustomPaddingWithSelf(1.1);
        }
    }
}