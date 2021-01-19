namespace AtomicTorch.CBND.CoreMod.Zones
{
    using AtomicTorch.CBND.GameApi;

    public class ZoneEventBoss : ProtoZoneDefault
    {
        [NotLocalizable]
        public override string Name => "Special - Event - Boss spawn";

        protected override void PrepareZone(ZoneScripts scripts)
        {
            // Special zone - no logic here
        }
    }
}