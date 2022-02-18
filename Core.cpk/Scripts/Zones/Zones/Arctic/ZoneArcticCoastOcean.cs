namespace AtomicTorch.CBND.CoreMod.Zones
{
    using AtomicTorch.CBND.GameApi;

    public class ZoneArcticCoastOcean : ProtoZoneDefault
    {
        [NotLocalizable]
        public override string Name => "Arctic - Coast - Ocean";

        protected override void PrepareZone(ZoneScripts scripts)
        {
            // minerals
                // none

            // loot
            scripts
                .Add(GetScript<SpawnLootStone>());

            // mobs
                // none
        }
    }
}