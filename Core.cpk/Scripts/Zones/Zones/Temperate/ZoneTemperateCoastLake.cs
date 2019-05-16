namespace AtomicTorch.CBND.CoreMod.Zones
{
    using AtomicTorch.CBND.GameApi;

    public class ZoneTemperateCoastLake : ProtoZoneDefault
    {
        [NotLocalizable]
        public override string Name => "Temperate - Coast - Lake";

        protected override void PrepareZone(ZoneScripts scripts)
        {
            // minerals
            scripts
                .Add(GetScript<SpawnResourceSand>().Configure(densityMultiplier: 0.75)); //slightly less sand for lakes

            // plants
            scripts
                .Add(GetScript<SpawnSugarcane>());

            // loot
            scripts
                .Add(GetScript<SpawnLootStone>());

            // mobs
            scripts
                .Add(GetScript<SpawnMobsCrab>().Configure(densityMultiplier: 0.8))
                .Add(GetScript<SpawnMobsRiverSnail>())
                .Add(GetScript<SpawnMobsTurtle>());
        }
    }
}