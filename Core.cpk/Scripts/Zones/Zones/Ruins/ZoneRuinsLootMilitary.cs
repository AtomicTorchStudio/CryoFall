namespace AtomicTorch.CBND.CoreMod.Zones
{
    using AtomicTorch.CBND.GameApi;

    public class ZoneRuinsLootMilitary : ProtoZoneDefault
    {
        [NotLocalizable]
        public override string Name => "Ruins - Loot - Military";

        protected override void PrepareZone(ZoneScripts scripts)
        {
            // loot
            scripts
                .Add(GetScript<SpawnLootRuinsMilitary>());
        }
    }
}