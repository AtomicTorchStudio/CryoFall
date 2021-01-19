namespace AtomicTorch.CBND.CoreMod.Zones.Spawn
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AtomicTorch.CBND.GameApi.Data.Zones;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;
    using CachedDict = System.Collections.Generic.IReadOnlyDictionary<
        GameEngine.Common.Primitives.Vector2Ushort,
        ProtoZoneSpawnScript.SpawnZoneArea>;

    public static class ServerSpawnZoneAreasHelper
    {
        private static readonly Dictionary<IServerZone, CachedDict>
            ZoneAreasCache = new();

        public static async ValueTask<CachedDict>
            ServerGetCachedZoneAreaAsync(
                IServerZone zone,
                Func<Task> callbackYieldIfOutOfTime)
        {
            if (!Api.IsEditor)
            {
                if (ZoneAreasCache.TryGetValue(zone, out var result))
                {
                    // found cached entry - cleanup it
                    foreach (var pair in result)
                    {
                        foreach (var list in pair.Value.WorldObjectsByPreset)
                        {
                            if (list.Value.Count > 0)
                            {
                                list.Value.Clear();
                            }
                        }
                    }

                    return result;
                }
            }

            var zoneChunks = await ZoneChunksHelper.CalculateZoneChunks(zone.QuadTree,
                                                                        ProtoZoneSpawnScript.SpawnZoneAreaSize,
                                                                        callbackYieldIfOutOfTime);
            var areas = new Dictionary<Vector2Ushort, ProtoZoneSpawnScript.SpawnZoneArea>(capacity: 16);
            await PopulateArea();

            if (!Api.IsEditor)
            {
                ZoneAreasCache[zone] = areas;
            }

            return areas;

            async Task PopulateArea()
            {
                // this is a heavy method so we will try to yield every few nodes to reduce the load
                const int defaultCounterToYieldValue = 100;
                var counterToYield = defaultCounterToYieldValue;

                foreach (var position in zone.AllPositions)
                {
                    await YieldIfOutOfTime();

                    var zoneChunkStartPosition = ProtoZoneSpawnScript.SpawnZoneArea.CalculateStartPosition(position);
                    if (areas.ContainsKey(zoneChunkStartPosition))
                    {
                        // area already exists
                        continue;
                    }

                    // create new zone area
                    var zoneChunk = zoneChunks[zoneChunkStartPosition];
                    var spawnZoneArea = new ProtoZoneSpawnScript.SpawnZoneArea(zoneChunkStartPosition, zoneChunk);
                    areas.Add(zoneChunkStartPosition, spawnZoneArea);
                }

                Task YieldIfOutOfTime()
                {
                    if (--counterToYield > 0)
                    {
                        return Task.CompletedTask;
                    }

                    counterToYield = defaultCounterToYieldValue;
                    return callbackYieldIfOutOfTime();
                }
            }
        }
    }
}