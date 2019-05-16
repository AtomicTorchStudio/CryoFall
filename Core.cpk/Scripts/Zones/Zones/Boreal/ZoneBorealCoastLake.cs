namespace AtomicTorch.CBND.CoreMod.Zones
{
    using AtomicTorch.CBND.GameApi;

    public class ZoneBorealCoastLake : ProtoZoneDefault
    {
        [NotLocalizable]
        public override string Name => "Boreal - Coast - Lake";

        protected override void PrepareZone(ZoneScripts scripts)
        {
            // minerals
            scripts
                .Add(GetScript<SpawnResourceSand>().Configure(densityMultiplier: 0.75)); //slightly less sand for lakes

            // loot
            scripts
                .Add(GetScript<SpawnLootStone>());

            // mobs
            scripts
                .Add(GetScript<SpawnMobsCrab>())
                .Add(GetScript<SpawnMobsRiverSnail>());
        }
    }
}