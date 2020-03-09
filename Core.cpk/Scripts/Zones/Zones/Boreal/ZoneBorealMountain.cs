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
                .Add(GetScript<SpawnResourceSulfur>())
                .Add(GetScript<SpawnResourceSalt>().Configure(densityMultiplier: 0.125));

            // mobs
            scripts
                .Add(GetScript<SpawnMobsCloakedLizard>())
                .Add(GetScript<SpawnMobsCrawler>());
        }
    }
}