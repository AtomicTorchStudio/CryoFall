namespace AtomicTorch.CBND.CoreMod.Zones
{
    using AtomicTorch.CBND.GameApi;

    public class ZoneTemperateCoastOcean : ProtoZoneDefault
    {
        [NotLocalizable]
        public override string Name => "Temperate - Coast - Ocean";

        protected override void PrepareZone(ZoneScripts scripts)
        {
            // minerals
            scripts
                .Add(GetScript<SpawnResourceSand>());

            // loot
            scripts
                .Add(GetScript<SpawnLootStone>());

            // mobs
            scripts
                .Add(GetScript<SpawnMobsCrab>().Configure(densityMultiplier: 0.8))
                .Add(GetScript<SpawnMobsStarfish>())
                .Add(GetScript<SpawnMobsTurtle>());
        }
    }
}