namespace AtomicTorch.CBND.CoreMod.Zones
{
    using AtomicTorch.CBND.GameApi;

    public class ZoneGenericRoads : ProtoZoneDefault
    {
        [NotLocalizable]
        public override string Name => "Generic - Roads";

        protected override void PrepareZone(ZoneScripts scripts)
        {
            // loot
            scripts
                .Add(GetScript<SpawnLootRoads>());
        }
    }
}