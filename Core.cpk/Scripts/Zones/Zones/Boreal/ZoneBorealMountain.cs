namespace AtomicTorch.CBND.CoreMod.Zones
{
    using AtomicTorch.CBND.GameApi;

    public class ZoneBorealMountain : ProtoZoneDefault
    {
        [NotLocalizable]
        public override string Name => "Boreal - Mountain";

        protected override void PrepareZone(ZoneScripts scripts)
        {
            // minerals
            scripts
                .Add(GetScript<SpawnResourceCopper>())
                .Add(GetScript<SpawnResourceIron>())
                .Add(GetScript<SpawnResourceStone>().Configure(densityMultiplier: 0.5))
                .Add(GetScript<SpawnResourceCoal>())
                .Add(GetScript<SpawnResourceSaltpeter>())
                .Add(GetScript<SpawnResourceSulfur>());

            // mobs
            scripts
                .Add(GetScript<SpawnMobsLizard>())
                .Add(GetScript<SpawnMobsCrawler>());
        }
    }
}