namespace AtomicTorch.CBND.CoreMod.Zones
{
    using AtomicTorch.CBND.GameApi;

    public class ZoneRuinsLootIndustrial : ProtoZoneDefault
    {
        [NotLocalizable]
        public override string Name => "Ruins - Loot - Industrial";

        protected override void PrepareZone(ZoneScripts scripts)
        {
            // loot
            scripts
                .Add(GetScript<SpawnLootRuinsIndustrial>());
        }
    }
}