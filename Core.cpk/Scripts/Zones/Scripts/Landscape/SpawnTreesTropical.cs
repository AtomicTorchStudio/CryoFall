namespace AtomicTorch.CBND.CoreMod.Zones
{
    using System;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Trees;
    using AtomicTorch.CBND.CoreMod.Triggers;

    public class SpawnTreesTropical : ProtoZoneSpawnScript
    {
        protected override void PrepareZoneSpawnScript(Triggers triggers, SpawnList spawnList)
        {
            triggers
                // trigger on world init
                .Add(GetTrigger<TriggerWorldInit>())
                // trigger on time interval
                .Add(GetTrigger<TriggerTimeInterval>().ConfigureForSpawn(TimeSpan.FromMinutes(15)));

            // all trees
            var trees = spawnList.CreatePreset(interval: 3.5, padding: 1.5)
                                 .Add<ObjectTreePalm>()
                                 .Add<ObjectTreeTropical>()
                                 .SetCustomPaddingWithSelf(2.1);

            // trees with stuff & food
            spawnList.CreatePreset(interval: 8, padding: 1.5)
                     .Add<ObjectTreeRubber>()
                     .Add<ObjectTreeBanana>()
                     .Add<ObjectTreeDurian>()
                     .SetCustomPaddingWithSelf(5)
                     .SetCustomPaddingWith(trees, 2.1);
        }
    }
}