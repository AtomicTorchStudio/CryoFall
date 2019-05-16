namespace AtomicTorch.CBND.CoreMod.Zones
{
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.GameApi;

    public class ZoneBorealForest : ProtoZoneDefault
    {
        [NotLocalizable]
        public override string Name => "Boreal - Forest";

        protected override void PrepareZone(ZoneScripts scripts)
        {
            if (!PveSystem.ServerIsPvE)
            {
                // spawn resource deposits on PvP servers
                scripts
                    .Add(GetScript<SpawnDepositGeothermalSpring>().Configure(densityMultiplier: 0.5));
            }

            // minerals
            scripts
                .Add(GetScript<SpawnResourcesGeneric>().Configure(densityMultiplier: 0.35))
                .Add(GetScript<SpawnResourceStone>().Configure(densityMultiplier: 0.1));

            // trees
            scripts
                .Add(GetScript<SpawnTreesBoreal>())
                // tree stumps, fallen trees, basically decorations that behave like trees in terms of chopping wood
                .Add(GetScript<SpawnStumpsTemperate>().Configure(densityMultiplier: 0.75));

            // other vegetation
            scripts
                .Add(GetScript<SpawnBushesForestBoreal>().Configure(densityMultiplier: 0.5))
                .Add(GetScript<SpawnMushroomsBoreal>().Configure(densityMultiplier: 0.30))
                .Add(GetScript<SpawnResourceHerbs>().Configure(densityMultiplier: 0.30));

            // loot
            scripts
                .Add(GetScript<SpawnLootGeneric>()); // loot (stone, grass, twigs)

            // mobs
            scripts
                // aggressive
                .Add(GetScript<SpawnMobsWolf>())
                .Add(GetScript<SpawnMobsWildBoar>())
                .Add(GetScript<SpawnMobsSnakeBlue>())
                .Add(GetScript<SpawnMobsBear>());
        }
    }
}