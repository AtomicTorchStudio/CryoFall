namespace AtomicTorch.CBND.CoreMod.Zones
{
    using AtomicTorch.CBND.GameApi;

    public class ZoneTemperateSalt : ProtoZoneDefault
    {
        [NotLocalizable]
        public override string Name => "Temperate - Salt pit";

        protected override void PrepareZone(ZoneScripts scripts)
        {
            // minerals
            scripts
                .Add(GetScript<SpawnResourceSalt>());

            // mobs
            scripts
                .Add(GetScript<SpawnMobsLizard>())
                .Add(GetScript<SpawnMobsCrawler>());
        }
    }
}