namespace AtomicTorch.CBND.CoreMod.Zones
{
    using AtomicTorch.CBND.GameApi;

    public class ZoneMobsScorpion : ProtoZoneDefault
    {
        [NotLocalizable]
        public override string Name => "Mobs - Scorpion";

        protected override void PrepareZone(ZoneScripts scripts)
        {
            scripts
                // when it's used separately by iself and not in ruins zone - we don't need such a high density, so set it lower
                .Add(GetScript<SpawnMobsScorpion>().Configure(densityMultiplier: 0.5));
        }
    }
}