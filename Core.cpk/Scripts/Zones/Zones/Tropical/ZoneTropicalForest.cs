namespace AtomicTorch.CBND.CoreMod.Zones
{
    using AtomicTorch.CBND.GameApi;

    public class ZoneTropicalForest : ProtoZoneDefault
    {
        [NotLocalizable]
        public override string Name => "Tropical - Forest";

        protected override void PrepareZone(ZoneScripts scripts)
        {
            // minerals
            scripts
                .Add(GetScript<SpawnResourcesGeneric>().Configure(densityMultiplier: 0.30))
                .Add(GetScript<SpawnResourceStone>().Configure(densityMultiplier: 0.20));

            // trees
            scripts
                .Add(GetScript<SpawnTreesTropical>());

            // loot
            scripts
                .Add(GetScript<SpawnLootGeneric>()); // loot (stone, grass, twigs)

            // other vegetation
            scripts
                .Add(GetScript<SpawnBushesForestTropical>().Configure(densityMultiplier: 0.5))
                .Add(GetScript<SpawnMushroomsTemperate>().Configure(densityMultiplier: 0.25))
                .Add(GetScript<SpawnResourceHerbs>().Configure(densityMultiplier: 0.25));

            // mobs
            scripts
                // passive
                .Add(GetScript<SpawnMobsChicken>())
                .Add(GetScript<SpawnMobsPangolin>())
                // aggressive
                .Add(GetScript<SpawnMobsTropicalBoar>().Configure(densityMultiplier: 0.8))
                .Add(GetScript<SpawnMobsSnakeGreen>().Configure(densityMultiplier: 0.5));
        }
    }
}