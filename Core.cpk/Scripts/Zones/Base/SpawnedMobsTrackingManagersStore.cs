namespace AtomicTorch.CBND.CoreMod.Zones
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Data.Zones;

    public static class SpawnedMobsTrackingManagersStore
    {
        private static readonly Dictionary<KeyValuePair<ProtoZoneSpawnScript, IServerZone>, SpawnedMobsTrackingManager>
            Managers = new();

        public static SpawnedMobsTrackingManager Get(ProtoZoneSpawnScript script, IServerZone serverZone)
        {
            var key = new KeyValuePair<ProtoZoneSpawnScript, IServerZone>(script, serverZone);
            if (!Managers.TryGetValue(key, out var mobsTrackingManager))
            {
                mobsTrackingManager = new SpawnedMobsTrackingManager();
                Managers[key] = mobsTrackingManager;
            }

            return mobsTrackingManager;
        }
    }
}