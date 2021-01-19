namespace AtomicTorch.CBND.CoreMod.Zones
{
    using AtomicTorch.CBND.GameApi;

    public class ZoneMobsSpitter : ProtoZoneDefault
    {
        [NotLocalizable]
        public override string Name => "Mobs - Spitters";

        protected override void PrepareZone(ZoneScripts scripts)
        {
            scripts
                .Add(GetScript<SpawnMobsSpitter>());
        }
    }
}