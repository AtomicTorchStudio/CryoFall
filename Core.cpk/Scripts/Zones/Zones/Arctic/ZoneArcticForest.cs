namespace AtomicTorch.CBND.CoreMod.Zones
{
    using AtomicTorch.CBND.GameApi;

    public class ZoneArcticForest : ProtoZoneDefault
    {
        [NotLocalizable]
        public override string Name => "Arctic - Forest";

        protected override void PrepareZone(ZoneScripts scripts)
        {
            // no pvp resources spawn, this area is a quiet "haven"

            // minerals
            scripts
                .Add(GetScript<SpawnResourceStone>().Configure(densityMultiplier: 0.15));

            // trees
            scripts
                .Add(GetScript<SpawnTreesArctic>());

            // loot
            scripts
                .Add(GetScript<SpawnLootPileWood>())
                .Add(GetScript<SpawnLootPileStone>().Configure(densityMultiplier: 0.2));

            // mobs
            scripts
                .Add(GetScript<SpawnMobsWolfPolar>())
                .Add(GetScript<SpawnMobsBearPolar>())
                .Add(GetScript<SpawnMobsBroodNest>());
        }
    }
}