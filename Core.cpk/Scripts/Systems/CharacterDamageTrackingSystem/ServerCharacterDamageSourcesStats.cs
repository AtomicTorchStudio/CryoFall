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
        // we store the damage sources for the last two minutes - 24 chunks per 5 seconds
        public const double ChunkDuration = 5;

        public const int ChunksCount = 24;

        public const double MaxStorageDuration = ChunksCount * ChunkDuration;

        private static readonly IGameServerService ServerGameService = Api.IsServer
                                                                           ? Api.Server.Game
                                                                           : null;

        private static readonly Dictionary<ServerDamageSourceEntry, double> TempRemoteStatsBuilderDictionary
            = new(capacity: 32);

        private readonly CycledArrayStorage<ServerDamageSourcesStatsChunk> storage
            = new(length: ChunksCount);

        public ServerCharacterDamageSourcesStats()
        {
            // preallocate chunks
            for (var i = 0; i < ChunksCount; i++)
            {
                this.storage.Add(new ServerDamageSourcesStatsChunk(0));
            }
        }

        public List<DamageSourceRemoteEntry> BuildRemoteStatsAfter(double timeThreshold)
        {
            var tempDictionary = TempRemoteStatsBuilderDictionary;
            try
            {
                foreach (var chunk in this.storage.CurrentEntries)
                {
                    if (chunk.StartingTime < timeThreshold)
                    {
                        // the chunk is too old
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
                    var entry = pair.Key;
                    list.Add(new DamageSourceRemoteEntry(entry.ProtoEntity,
                                                         entry.Name,
                                                         entry.ClanTag,
                                                         fraction: (float)fraction));
                }

                return list;
            }
            finally
            {
                tempDictionary.Clear();
            }
        }

        public List<DamageSourceRemoteEntry> BuildRemoteStatsBeforeDeath(double deathTime)
        {
            var timeThreshold = deathTime - MaxStorageDuration;
            return this.BuildRemoteStatsAfter(timeThreshold);
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
                = new(capacity: 6);

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