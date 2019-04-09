namespace AtomicTorch.CBND.CoreMod.Zones
{
    using AtomicTorch.CBND.GameApi;

    public class ZoneSpecialPlayerSpawn : ProtoZoneDefault
    {
        [NotLocalizable]
        public override string Name => "Special - Player spawn";

        protected override void PrepareZone(ZoneScripts scripts)
        {
            // Special zone - no logic here
            // See ServerPlayerSpawnManager which uses this zone as the players spawn positions provider.
        }
    }
}