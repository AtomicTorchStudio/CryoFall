namespace AtomicTorch.CBND.CoreMod.Systems.CharacterDamageTrackingSystem
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using JetBrains.Annotations;

    public class CharacterDamageTrackingSystem : ProtoSystem<CharacterDamageTrackingSystem>
    {
        private const string DatabaseKeyPlayerDamageSourceStats = "PlayerDamageSourceStats";

        private static readonly IGameServerService ServerGameService = IsServer
                                                                           ? Api.Server.Game
                                                                           : null;

        private static Dictionary<ICharacter, ServerCharacterDamageSourcesStats>
            serverPlayerCharacterDamageSourcesStats;

        [NotLocalizable]
        public override string Name => "Damage tracking system";

        [ItemCanBeNull]
        public static async Task<List<DamageSourceRemoteEntry>> ClientGetDamageTrackingStatsAsync()
        {
            var result = await Instance.CallServer(_ => _.ServerRemote_GetDamageTrackingStatsAsync());
            return result?.OrderByDescending(e => e.Fraction)
                         .ToList();
        }

        public static void ServerClearStats(ICharacter character)
        {
            if (character.IsNpc)
            {
                return;
            }

            if (serverPlayerCharacterDamageSourcesStats.TryGetValue(character, out var damageSourcesStats))
            {
                damageSourcesStats.ClearStats();
            }
        }

        [CanBeNull]
        public static List<DamageSourceRemoteEntry> ServerGetDamageSourcesAfterTime(
            ICharacter character,
            double afterTime)
        {
            if (!serverPlayerCharacterDamageSourcesStats.TryGetValue(character, out var stats))
            {
                // no damage stats
                return null;
            }

            return stats.BuildRemoteStatsAfter(afterTime);
        }

        [CanBeNull]
        public static List<DamageSourceRemoteEntry> ServerGetDamageSourcesLatest(ICharacter character)
        {
            if (!serverPlayerCharacterDamageSourcesStats.TryGetValue(character, out var stats))
            {
                // no damage stats
                return null;
            }

            // get the time until the damage stats are interested
            var privateState = PlayerCharacter.GetPrivateState(character);
            var publicState = PlayerCharacter.GetPublicState(character);
            var untilTime = publicState.IsDead && privateState.LastDeathTime.HasValue
                                ? privateState.LastDeathTime.Value
                                : 0; // get all the stored damage sources as we don't have the death time

            return stats.BuildRemoteStatsBeforeDeath(untilTime);
        }

        /// <summary>
        /// Returns the PvP damage fraction for the last death in range [0;1].
        /// </summary>
        public static double ServerGetPvPdamagePercent(ICharacter character)
        {
            var percentOfPvPdamage = 0.0;
            var damageSources = ServerGetDamageSourcesLatest(character);
            if (damageSources is null)
            {
                return 0;
            }

            foreach (var entry in damageSources)
            {
                switch (entry.ProtoEntity)
                {
                    case PlayerCharacter:
                        percentOfPvPdamage += entry.Fraction;
                        break;

                    case StatusEffectRadiationPoisoning:
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

            if (!serverPlayerCharacterDamageSourcesStats.TryGetValue(damagedCharacter, out var damageSourcesStats))
            {
                damageSourcesStats = new ServerCharacterDamageSourcesStats();
                serverPlayerCharacterDamageSourcesStats[damagedCharacter] = damageSourcesStats;
            }

            damageSourcesStats.RegisterDamage(damage, damageSourceEntry);
        }

        protected override void PrepareSystem()
        {
            if (IsClient)
            {
                return;
            }

            if (Server.Database.TryGet(nameof(CharacterDamageTrackingSystem),
                                           DatabaseKeyPlayerDamageSourceStats,
                                           out serverPlayerCharacterDamageSourcesStats))
            {
                return;
            }

            serverPlayerCharacterDamageSourcesStats =
                new Dictionary<ICharacter, ServerCharacterDamageSourcesStats>();
            Server.Database.Set(nameof(CharacterDamageTrackingSystem),
                                    DatabaseKeyPlayerDamageSourceStats,
                                    serverPlayerCharacterDamageSourcesStats);
        }

        [RemoteCallSettings(timeInterval: 2)]
        [CanBeNull]
        private List<DamageSourceRemoteEntry> ServerRemote_GetDamageTrackingStatsAsync()
        {
            return ServerGetDamageSourcesLatest(ServerRemoteContext.Character);
        }
    }
}