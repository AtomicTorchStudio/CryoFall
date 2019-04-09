namespace AtomicTorch.CBND.CoreMod.Zones
{
    using AtomicTorch.CBND.GameApi;

    public class ZoneTemperateMeadow : ProtoZoneDefault
    {
        [NotLocalizable]
        public override string Name => "Temperate - Meadow";

        protected override void PrepareZone(ZoneScripts scripts)
        {
            // different vegetation
            scripts
                .Add(GetScript<SpawnBushesForestTemperate>())
                .Add(GetScript<SpawnMushroomsTemperate>())
                // default density for herbs compared to forest, we want them to spawn here a lot
                .Add(GetScript<SpawnResourceHerbs>());

            // loot
            scripts
                .Add(GetScript<SpawnLootGeneric>().Configure(densityMultiplier: 1.35)); // loot (stone, grass, twigs)

            // mobs
            scripts
                .Add(GetScript<SpawnMobsWolf>())
                .Add(GetScript<SpawnMobsChicken>())
                .Add(GetScript<SpawnMobsWildBoar>())
                .Add(GetScript<SpawnMobsSnakeGreen>());
        }
    }
}