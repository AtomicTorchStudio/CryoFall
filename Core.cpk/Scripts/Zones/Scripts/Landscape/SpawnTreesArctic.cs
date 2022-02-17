namespace AtomicTorch.CBND.CoreMod.Zones
{
    using System;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.SmallGatherables;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Trees;
    using AtomicTorch.CBND.CoreMod.Triggers;

    public class SpawnTreesArctic : ProtoZoneSpawnScript
    {
        protected override void PrepareZoneSpawnScript(Triggers triggers, SpawnList spawnList)
        {
            triggers
                // trigger on world init
                .Add(GetTrigger<TriggerWorldInit>())
                // trigger on time interval
                .Add(GetTrigger<TriggerTimeInterval>().ConfigureForSpawn(TimeSpan.FromMinutes(30)));

            // dead trees, stumps, etc.
            var deadTrees = spawnList.CreatePreset(interval: 13, padding: 1.5)
                                     .Add<ObjectTreeDeadArctic1>()
                                     .Add<ObjectTreeDeadArctic2>()
                                     .Add<ObjectTreeStumpArctic>()
                                     .Add<ObjectTreeFallenArctic>()
                                     .SetCustomPaddingWithSelf(9);

            // plants
            spawnList.CreatePreset(interval: 10, padding: 1.5)
                     .Add<ObjectSmallBushArctic1>()
                     .Add<ObjectSmallBushArctic2>()
                     .SetCustomPaddingWithSelf(5);

            // clusters of pines
            spawnList.CreatePreset(interval: 6, padding: 1.5)
                     .Add<ObjectTreePineArctic>()
                     .SetCustomPaddingWith(deadTrees, 5)
                     // not directly near (left, right, etc.), but diagonally - ok!
                     .SetCustomPaddingWithSelf(1.1);
        }
    }
}