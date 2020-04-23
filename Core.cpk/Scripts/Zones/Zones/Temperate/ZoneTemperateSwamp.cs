namespace AtomicTorch.CBND.CoreMod.Zones
{
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.GameApi;

    public class ZoneTemperateSwamp : ProtoZoneDefault
    {
        [NotLocalizable]
        public override string Name => "Temperate - Swamp";

        protected override void PrepareZone(ZoneScripts scripts)
        {
            if (!PveSystem.ServerIsPvE)
            {
                // spawn resource deposits on PvP servers
                scripts
                    .Add(GetScript<SpawnDepositOilSeep>().Configure(densityMultiplier: 0.7));
            }

            // minerals
            scripts
                .Add(GetScript<SpawnResourcesGeneric>().Configure(densityMultiplier: 0.30))
                .Add(GetScript<SpawnResourceStone>().Configure(densityMultiplier: 0.1))
                .Add(GetScript<SpawnResourceCoal>().Configure(densityMultiplier: 0.1));

            // trees
            scripts
                .Add(GetScript<SpawnTreesSwamp>());

            // other vegetation
            scripts
                .Add(GetScript<SpawnBushesForestTemperate>().Configure(densityMultiplier: 0.25))
                .Add(GetScript<SpawnMushroomsTemperate>().Configure(densityMultiplier: 0.25))
                .Add(GetScript<SpawnResourceHerbs>().Configure(densityMultiplier: 0.25))
                .Add(GetScript<SpawnSugarcane>().Configure(densityMultiplier: 0.2));

            // swamp exclusive blue herb
            scripts
                .Add(GetScript<SpawnResourceHerbBlue>());

            // loot
            scripts
                .Add(GetScript<SpawnLootGeneric>().Configure(densityMultiplier: 0.5)); // loot (stone, grass, twigs)

            // mobs
            scripts
                // passive
                .Add(GetScript<SpawnMobsRiverSnail>())
                // aggressive
                .Add(GetScript<SpawnMobsBlackBeetle>())
                .Add(GetScript<SpawnMobsBurrower>())
                .Add(GetScript<SpawnMobsSnakeGreen>())
                .Add(GetScript<SpawnMobsWildBoar>());
        }
    }
}