namespace AtomicTorch.CBND.CoreMod.Zones
{
    using AtomicTorch.CBND.GameApi;

    public class ZoneEventBossPragmiumQueen : ProtoZoneDefault
    {
        [NotLocalizable]
        public override string Name => "Event - Boss Pragmium Queen";

        protected override void PrepareZone(ZoneScripts scripts)
        {
            // Special zone - no logic here
        }
    }
}