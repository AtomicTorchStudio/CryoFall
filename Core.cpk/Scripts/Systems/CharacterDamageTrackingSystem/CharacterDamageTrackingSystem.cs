namespace AtomicTorch.CBND.CoreMod.Systems.CharacterDamageTrackingSystem
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.DataStructures;
    using JetBrains.Annotations;

    public class CharacterDamageTrackingSystem : ProtoSystem<CharacterDamageTrackingSystem>
    {
        private static readonly Dictionary<ICharacter, ServerCharacterDamageSourcesStats>
            DictionaryCharacterDamageSourcesStats
                = new Dictionary<ICharacter, ServerCharacterDamageSourcesStats>();

        private static readonly IGameServerService ServerGameService = IsServer
                                                                           ? Api.Server.Game
                                                                           : null;

        [NotLocalizable]
        public override string Name => "Damage tracking system";

        /// <summary>
        /// Please note - the result could be a null list.
        /// </summary>
        public static async Task<List<DamageSourceRemoteEntry>> ClientGetDamageTrackingStatsAsync()
        {
            var result = await Instance.CallServer(_ => _.ServerRemote_GetDamageTrackingStatsAsync());
            return result?.OrderByDescending(e => e.Percent)
                         .ToList();
        }

        public static void ServerClearStats(ICharacter character)
        {
            if (character.IsNpc)
            {
                return;
            }

            if (DictionaryCharacterDamageSourcesStats.TryGetValue(character, out var damageSourcesStats))
            {
                damageSourcesStats.ClearStats();
            }
        }

        [CanBeNull]
        public static List<DamageSourceRemoteEntry> ServerGetDamageSources(ICharacter character)
        {
            if (!DictionaryCharacterDamageSourcesStats.TryGetValue(character, out var stats))
            {
                // no damage stats
                return null;
            }

            // get the time until the damage stats are interested
            var privateState = PlayerCharacter.GetPrivateState(character);
            var publicState = PlayerCharacter.GetPublicState(character);
            var untilTime = publicState.IsDead && privateState.LastDeathTime.HasValue
                                ? privateState.LastDeathTime.Value
                                : ServerGameService.FrameTime;

            return stats.BuildRemoteStats(untilTime);
        }

        /// <summary>
        /// Returns the PvP damage fraction for the last death in range [0;1].
        /// </summary>
        public static double ServerGetPvPdamagePercent(ICharacter character)
        {
            var percentOfPvPdamage = 0.0;
            var damageSources = ServerGetDamageSources(character);
            if (damageSources == null)
            {
                return 0;
            }

            foreach (var entry in damageSources)
            {
                switch (entry.ProtoEntity)
                {
                    case PlayerCharacter _:
                        percentOfPvPdamage += entry.Percent;
                        break;

                    case StatusEffectRadiationPoisoning _:
                        // special case - death in radtown
                        // we want player to drop loot in that case even if there were PvP damage
                        return 0;
                }
            }

            return percentOfPvPdamage;
        }

        public static void ServerRegisterDamage(
            double damage,
            ICharacter damagedCharacter,
            ServerDamageSourceEntry damageSourceEntry)
        {
            if (damagedCharacter.IsNpc
                || damage <= 0)
            {
                return;
            }

            if (!DictionaryCharacterDamageSourcesStats.TryGetValue(damagedCharacter, out var damageSourcesStats))
            {
                damageSourcesStats = new ServerCharacterDamageSourcesStats();
                DictionaryCharacterDamageSourcesStats[damagedCharacter] = damageSourcesStats;
            }

            damageSourcesStats.RegisterDamage(damage, damageSourceEntry);
        }

        /// <summary>
        /// Please note - the result could be a null list.
        /// </summary>
        /// <returns></returns>
        [CanBeNull]
        private List<DamageSourceRemoteEntry> ServerRemote_GetDamageTrackingStatsAsync()
        {
            return ServerGetDamageSources(ServerRemoteContext.Character);
        }

        private class ServerCharacterDamageSourcesStats
        {
            // we store the damage sources for the last two minutes - 12 chunks per 10 seconds
            private const double ChunkDuration = 10;

            private const int ChunksCount = 12;

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
                        var percent = pair.Value / totalDamage;
                        list.Add(new DamageSourceRemoteEntry(pair.Key.ProtoEntity,
                                                             pair.Key.Name,
                                                             percent: (float)percent));
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

            private class ServerDamageSourcesStatsChunk
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
}