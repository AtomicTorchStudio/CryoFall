namespace AtomicTorch.CBND.CoreMod.Zones
{
    using AtomicTorch.CBND.GameApi;

    public class ZoneSpecialRadiationTier2 : ProtoZoneDefault
    {
        [NotLocalizable]
        public override string Name => "Special - Radiation (Tier 2)";

        protected override void PrepareZone(ZoneScripts scripts)
        {
            // Special zone - no logic here
            // See StatusEffectRadiation which uses this zone to determine what characters are affected by radiation
        }
    }
}