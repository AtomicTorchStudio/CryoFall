namespace AtomicTorch.CBND.CoreMod.Zones
{
    using AtomicTorch.CBND.GameApi;

    public class ZoneRuinsMobsNormal : ProtoZoneDefault
    {
        [NotLocalizable]
        public override string Name => "Ruins - Mobs - Normal";

        protected override void PrepareZone(ZoneScripts scripts)
        {
            scripts
                .Add(GetScript<SpawnMobsRuinsNormal>());
        }
    }
}