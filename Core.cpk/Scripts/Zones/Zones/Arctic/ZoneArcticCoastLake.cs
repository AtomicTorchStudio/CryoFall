namespace AtomicTorch.CBND.CoreMod.Zones
{
    using AtomicTorch.CBND.GameApi;

    public class ZoneArcticCoastLake : ProtoZoneDefault
    {
        [NotLocalizable]
        public override string Name => "Arctic - Coast - Lake";

        protected override void PrepareZone(ZoneScripts scripts)
        {
            // minerals
            scripts
                .Add(GetScript<SpawnResourceSand>().Configure(densityMultiplier: 0.5)); //less sand for arctic lakes

            // loot
            scripts
                .Add(GetScript<SpawnLootStone>());

            // mobs
                // none
        }
    }
}