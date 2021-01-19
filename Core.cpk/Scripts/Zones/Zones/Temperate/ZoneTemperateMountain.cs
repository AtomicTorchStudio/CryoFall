﻿namespace AtomicTorch.CBND.CoreMod.Zones
{
    using AtomicTorch.CBND.GameApi;

    public class ZoneTemperateMountain : ProtoZoneDefault
    {
        [NotLocalizable]
        public override string Name => "Temperate - Mountain";

        protected override void PrepareZone(ZoneScripts scripts)
        {
            // minerals
            scripts
                .Add(GetScript<SpawnResourceCopper>())
                .Add(GetScript<SpawnResourceIron>())
                .Add(GetScript<SpawnResourceStone>())
                .Add(GetScript<SpawnResourceSaltpeter>())
                .Add(GetScript<SpawnResourceSulfur>());

            // loot
            scripts
                .Add(GetScript<SpawnLootPileMinerals>());

            // mobs
            scripts
                .Add(GetScript<SpawnMobsCloakedLizard>().Configure(densityMultiplier: 0.5))
                .Add(GetScript<SpawnMobsCrawler>());
        }
    }
}