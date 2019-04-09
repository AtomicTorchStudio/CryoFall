namespace AtomicTorch.CBND.CoreMod.Zones
{
    using AtomicTorch.CBND.GameApi;

    public class ZoneRuinsLootLaboratory : ProtoZoneDefault
    {
        [NotLocalizable]
        public override string Name => "Ruins - Loot - Laboratory";

        protected override void PrepareZone(ZoneScripts scripts)
        {
            // loot
            scripts
                .Add(GetScript<SpawnLootRuinsLaboratory>());
        }
    }
}