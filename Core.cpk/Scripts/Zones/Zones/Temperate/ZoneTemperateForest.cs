namespace AtomicTorch.CBND.CoreMod.Zones
{
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.GameApi;

    public class ZoneTemperateForest : ProtoZoneDefault
    {
        [NotLocalizable]
        public override string Name => "Temperate - Forest";

        protected override void PrepareZone(ZoneScripts scripts)
        {
            if (!PveSystem.ServerIsPvE)
            {
                // spawn resource deposits on PvP servers
                scripts
                    .Add(GetScript<SpawnDepositGeothermalSpring>().Configure(densityMultiplier: 0.3));
            }

            // minerals
            scripts
                .Add(GetScript<SpawnResourcesGeneric>().Configure(densityMultiplier: 0.30))
                .Add(GetScript<SpawnResourceStone>().Configure(densityMultiplier: 0.1));

            // trees
            scripts
                // trees (in the end, because they're very dense and will interfere with spawn of other stuff)
                .Add(GetScript<SpawnTreesTemperate>())
                // tree stumps, fallen trees, basically decorations that behave like trees in terms of chopping wood
                .Add(GetScript<SpawnStumpsTemperate>());

            // other vegetation
            scripts
                .Add(GetScript<SpawnBushesForestTemperate>().Configure(densityMultiplier: 0.5))
                .Add(GetScript<SpawnMushroomsTemperate>().Configure(densityMultiplier: 0.25))
                .Add(GetScript<SpawnResourceHerbs>().Configure(densityMultiplier: 0.25));

            // loot
            scripts
                .Add(GetScript<SpawnLootGeneric>()) // loot (stone, grass, twigs)
                .Add(GetScript<SpawnLootPileWood>())
                .Add(GetScript<SpawnLootPileStone>().Configure(densityMultiplier: 0.2));

            // mobs
            scripts
                // passive
                .Add(GetScript<SpawnMobsChicken>())
                // aggressive
                .Add(GetScript<SpawnMobsWolf>().Configure(densityMultiplier: 0.8))
                .Add(GetScript<SpawnMobsWildBoar>().Configure(densityMultiplier: 0.8))
                .Add(GetScript<SpawnMobsSnakeGreen>().Configure(densityMultiplier: 0.8));
        }
    }
}