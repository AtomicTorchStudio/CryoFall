namespace AtomicTorch.CBND.CoreMod.Zones
{
    using AtomicTorch.CBND.GameApi;

    public class ZoneTropicalMountain : ProtoZoneDefault
    {
        [NotLocalizable]
        public override string Name => "Tropical - Mountain";

        protected override void PrepareZone(ZoneScripts scripts)
        {
            // minerals
            scripts
                .Add(GetScript<SpawnResourceCopper>())
                .Add(GetScript<SpawnResourceIron>())
                .Add(GetScript<SpawnResourceStone>())
                .Add(GetScript<SpawnResourceCoal>().Configure(densityMultiplier: 0.7)) // less coal than boreal, but still still available
                .Add(GetScript<SpawnResourceSaltpeter>())
                .Add(GetScript<SpawnResourceSulfur>());

            // loot
            scripts
                .Add(GetScript<SpawnLootPileMinerals>());

            // mobs
            scripts
                .Add(GetScript<SpawnMobsCrawler>());
        }
    }
}