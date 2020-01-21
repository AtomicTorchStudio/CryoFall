namespace AtomicTorch.CBND.CoreMod.Zones
{
    using AtomicTorch.CBND.GameApi;

    public class ZoneTemperateClay : ProtoZoneDefault
    {
        [NotLocalizable]
        public override string Name => "Temperate - Clay pit";

        protected override void PrepareZone(ZoneScripts scripts)
        {
            // minerals
            scripts
                .Add(GetScript<SpawnResourceClay>());

            // mobs
            scripts
                .Add(GetScript<SpawnMobsCloakedLizard>())
                .Add(GetScript<SpawnMobsCrawler>());
        }
    }
}