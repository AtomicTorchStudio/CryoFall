namespace AtomicTorch.CBND.CoreMod.Systems.CharacterDamageTrackingSystem
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.DataStructures;

    // TODO: don't require IRemoteCallParameter when [Serializable] is present
    [Serializable]
    public class ServerCharacterDamageSourcesStats : IRemoteCallParameter
    {
        // we store the damage sources for the last two minutes - 12 chunks per 10 seconds
        private const double ChunkDuration = 10;

        private const int ChunksCount = 12;

        private static readonly IGameServerService ServerGameService = Api.IsServer
                                                                           ? Api.Server.Game
                                                                           : null;

        private static readonly Dictionary<ServerDamageSourceEntry, double> TempRemoteStatsBuilderDictionary
            = new Dictionary<ServerDamageSourceEntry, double>(capacity: 32);

        private readonly CycledArrayStorage<ServerDamageSourcesStatsChunk> storage
            = new CycledArrayStorage<ServerDamageSourcesStatsChunk>(length: ChunksCount);

        public ServerCharacterDamageSourcesStats()
        {
            // preallocate chunks
            for (var i = 0; i < ChunksCount; i++)
            {
                this.storage.Add(new ServerDamageSourcesStatsChunk(0));
            }
        }

        public List<DamageSourceRemoteEntry> BuildRemoteStats(double untilTime)
        {
            var tempDictionary = TempRemoteStatsBuilderDictionary;
            try
            {
                var timeThreshold = untilTime - ChunkDuration * ChunksCount;
                foreach (var chunk in this.storage.CurrentEntries)
                {
                    if (chunk.StartingTime < timeThreshold)
                    {
                        // too old chunk
                        continue;
                    }

                    // combine the info from this chunk to the result
                    foreach (var pair in chunk.DictionaryDamageBySource)
                    {
                        var damage = pair.Value;
                        if (tempDictionary.TryGetValue(pair.Key, out var existingDamage))
                        {
                            damage += existingDamage;
                        }

                        tempDictionary[pair.Key] = damage;
                    }
                }

                var totalDamage = 0.0;
                foreach (var pair in tempDictionary)
                {
                    totalDamage += pair.Value;
                }

                if (tempDictionary.Count == 0)
                {
                    // no damage stats
                    return null;
                }

                var list = new List<DamageSourceRemoteEntry>(capacity: tempDictionary.Count);
                foreach (var pair in tempDictionary)
                {
                    var fraction = pair.Value / totalDamage;
                    list.Add(new DamageSourceRemoteEntry(pair.Key.ProtoEntity,
                                                         pair.Key.Name,
                                                         fraction: (float)fraction));
                }

                return list;
            }
            finally
            {
                tempDictionary.Clear();
            }
        }

        public void ClearStats()
        {
            foreach (var entry in this.storage.CurrentEntries)
            {
                entry.Clear();
            }
        }

        public void RegisterDamage(double damage, in ServerDamageSourceEntry damageSourceEntry)
        {
            var lastEntry = this.storage.LastOrDefault();
            var frameTime = ServerGameService.FrameTime;
            if (frameTime > lastEntry.StartingTime + ChunkDuration)
            {
                // need to use the next chunk
                this.storage.MoveNext();
                lastEntry = this.storage.LastOrDefault();
                // setup the chunk
                lastEntry.Clear();
                lastEntry.StartingTime = frameTime;
            }

            lastEntry.RegisterDamage(damage, damageSourceEntry);
        }

        [Serializable]
        private class ServerDamageSourcesStatsChunk : IRemoteCallParameter
        {
            public readonly Dictionary<ServerDamageSourceEntry, double> DictionaryDamageBySource
                = new Dictionary<ServerDamageSourceEntry, double>(capacity: 6);

            public double StartingTime;

            public ServerDamageSourcesStatsChunk(double startingTime)
            {
                this.StartingTime = startingTime;
            }

            public void Clear()
            {
                this.StartingTime = 0;
                this.DictionaryDamageBySource.Clear();
            }

            public void RegisterDamage(double damage, in ServerDamageSourceEntry damageSourceEntry)
            {
                if (this.DictionaryDamageBySource.TryGetValue(damageSourceEntry, out var existingDamage))
                {
                    damage += existingDamage;
                }

                this.DictionaryDamageBySource[damageSourceEntry] = damage;
            }
        }
    }
}