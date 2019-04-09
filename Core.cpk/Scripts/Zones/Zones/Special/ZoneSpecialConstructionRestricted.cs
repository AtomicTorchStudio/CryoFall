namespace AtomicTorch.CBND.CoreMod.Zones
{
    using AtomicTorch.CBND.GameApi;

    public class ZoneSpecialConstructionRestricted : ProtoZoneDefault
    {
        public static ZoneSpecialConstructionRestricted Instance { get; private set; }

        [NotLocalizable]
        public override string Name => "Special - Restricted Construction";

        protected override void PrepareZone(ZoneScripts scripts)
        {
            // Special zone - no logic here, it is handled by external implementation
            // Used to restrict building in certain map areas.
            Instance = this;
        }
    }
}