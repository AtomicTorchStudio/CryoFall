namespace AtomicTorch.CBND.CoreMod.Zones
{
    using System;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Loot;
    using AtomicTorch.CBND.CoreMod.Triggers;

    public class SpawnLootRuinsLaboratory : ProtoZoneSpawnScript
    {
        public override bool CanSpawnIfPlayersNearby => true;

        protected override void PrepareZoneSpawnScript(Triggers triggers, SpawnList spawnList)
        {
            triggers
                // trigger on world init
                .Add(GetTrigger<TriggerWorldInit>())
                // trigger on time interval
                .Add(GetTrigger<TriggerTimeInterval>().Configure(TimeSpan.FromMinutes(30)));

            spawnList.CreatePreset(interval: 6, padding: 1)
                     .Add<ObjectLootPileGarbageLarge>(weight: 4)
                     .Add<ObjectLootCrateHightech>(weight: 3)
                     .Add<ObjectLootCrateMedical>(weight: 1)
                     .SetCustomPaddingWithSelf(9);
        }
    }
}