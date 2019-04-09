namespace AtomicTorch.CBND.CoreMod.Zones
{
    using AtomicTorch.CBND.GameApi;

    public class ZoneRuinsLootResidential : ProtoZoneDefault
    {
        [NotLocalizable]
        public override string Name => "Ruins - Loot - Residential";

        protected override void PrepareZone(ZoneScripts scripts)
        {
            // loot
            scripts
                .Add(GetScript<SpawnLootRuinsResidential>());
        }
    }
}