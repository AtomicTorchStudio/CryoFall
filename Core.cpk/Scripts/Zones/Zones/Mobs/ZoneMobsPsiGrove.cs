namespace AtomicTorch.CBND.CoreMod.Zones
{
    using AtomicTorch.CBND.GameApi;

    public class ZoneMobsPsiGrove : ProtoZoneDefault
    {
        [NotLocalizable]
        public override string Name => "Mobs - Psi grove";

        protected override void PrepareZone(ZoneScripts scripts)
        {
            scripts
                .Add(GetScript<SpawnMobsPsiGrove>())
                // TODO: replace this in A30 Update with a separate zone to spawn scorpions
                .Add(GetScript<SpawnMobsScorpion>().Configure(densityMultiplier: 0.5));
        }
    }
}