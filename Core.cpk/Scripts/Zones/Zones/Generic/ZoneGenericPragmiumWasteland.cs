namespace AtomicTorch.CBND.CoreMod.Zones
{
    using AtomicTorch.CBND.CoreMod.Zones.Special;
    using AtomicTorch.CBND.GameApi;

    public class ZoneGenericPragmiumWasteland : ProtoZoneDefault
    {
        [NotLocalizable]
        public override string Name => "Generic - Pragmium wasteland";

        protected override void PrepareZone(ZoneScripts scripts)
        {
            // minerals and mobs
            scripts
                .Add(GetScript<SpawnPragmiumWasteland>());
        }
    }
}