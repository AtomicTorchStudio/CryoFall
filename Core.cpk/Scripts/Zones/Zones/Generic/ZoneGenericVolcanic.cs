namespace AtomicTorch.CBND.CoreMod.Zones
{
    using AtomicTorch.CBND.GameApi;

    public class ZoneGenericVolcanic : ProtoZoneDefault
    {
        [NotLocalizable]
        public override string Name => "Generic - Volcanic";

        protected override void PrepareZone(ZoneScripts scripts)
        {
            // minerals
            scripts
                .Add(GetScript<SpawnResourceCopper>().Configure(densityMultiplier: 0.25))
                .Add(GetScript<SpawnResourceIron>().Configure(densityMultiplier: 0.25))
                .Add(GetScript<SpawnResourceStone>().Configure(densityMultiplier: 0.25))
                .Add(GetScript<SpawnResourceSaltpeter>().Configure(densityMultiplier: 0.25))
                .Add(GetScript<SpawnResourceSulfur>().Configure(densityMultiplier: 0.25));

            // mobs
            scripts
                .Add(GetScript<SpawnMobsVolcanic>());
        }
    }
}