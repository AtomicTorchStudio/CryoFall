namespace AtomicTorch.CBND.CoreMod.Systems.FactionLeaderboardSystem
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.Systems.FactionLeaderboardSystem.Metrics;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Extensions;
    using JetBrains.Annotations;

    public class FactionLeaderboardSystem : ProtoSystem<FactionLeaderboardSystem>
    {
        public const uint ServerUpdateInterval = 60 * 60; // once an hour

        public const double TotalScoreMultiplier = 0.1;

        private const string DatabaseKeyLastUpdateTime = "LastUpdateTime";

        private static double serverLastUpdateTime;

        private static ProtoFactionScoreMetric[] serverMetrics;

        public static event Action ClientLeaderboardUpdatedHandler;

        public static double ClientNextLeaderboardUpdateTime { get; private set; }

        public static Task<IReadOnlyDictionary<ProtoFactionScoreMetric, uint>> ClientGetFactionScoreMetricsAsync(
            string clanTag)
        {
            return Instance.CallServer(_ => _.ServerRemote_GetScoreMetrics(clanTag));
        }

        public static void ServerForceUpdateLeaderboard()
        {
            ServerUpdateLeaderboard();
        }

        protected override void PrepareSystem()
        {
            if (IsClient)
            {
                return;
            }

            serverMetrics = Api.FindProtoEntities<ProtoFactionScoreMetric>()
                               .ToArray();

            foreach (var metric in serverMetrics)
            {
                metric.ServerInitialize();

                if (metric.PowerCoefficient is <= 0 or > 1)
                {
                    Logger.Error("Incorrect value of "
                                 + nameof(ProtoFactionScoreMetric.PowerCoefficient)
                                 + " in "
                                 + metric.GetType().FullName
                                 + ": the power must be within (0;1] range");
                }
            }

            if (!Server.Database.TryGet(nameof(FactionLeaderboardSystem),
                                        DatabaseKeyLastUpdateTime,
                                        out serverLastUpdateTime))
            {
                serverLastUpdateTime = Server.Game.FrameTime;
                Server.Database.Set(nameof(FactionLeaderboardSystem),
                                    DatabaseKeyLastUpdateTime,
                                    serverLastUpdateTime);
            }

            TriggerTimeInterval.ServerConfigureAndRegister(
                TimeSpan.FromSeconds(1),
                ServerUpdate,
                "System." + this.ShortId);

            Server.Characters.PlayerOnlineStateChanged += this.ServerPlayerOnlineStateChangedHandler;
        }

        private static void ServerUpdate()
        {
            if (Api.Server.Game.FrameTime < serverLastUpdateTime + ServerUpdateInterval)
            {
                return;
            }

            // time for updating the leaderboard
            ServerUpdateLeaderboard();
        }

        private static void ServerUpdateLeaderboard()
        {
            var stopwatch = Stopwatch.StartNew();
            serverLastUpdateTime = Api.Server.Game.FrameTime;
            Server.Database.Set(nameof(FactionLeaderboardSystem),
                                DatabaseKeyLastUpdateTime,
                                serverLastUpdateTime);

            var allFactions = new List<ILogicObject>();
            Api.GetProtoEntity<Faction>()
               .GetAllGameObjects(allFactions);

            var selectedFactionsByScore = new List<TempFactionScoreEntry>(allFactions.Count);
            foreach (var faction in allFactions)
            {
                if (FactionSystem.SharedGetFactionKind(faction)
                    == FactionKind.Public)
                {
                    // only private factions are participating in the leaderboard
                    continue;
                }

                var totalScore = 0.0;
                var scoreByMetric = new Dictionary<ProtoFactionScoreMetric, uint>(capacity: serverMetrics.Length);
                foreach (var metric in serverMetrics)
                {
                    var value = metric.ServerGetCurrentValue(faction);
                    value *= metric.FinalScoreCoefficient;

                    if (metric.PowerCoefficient is > 0 and < 1)
                    {
                        value = Math.Pow(value, metric.PowerCoefficient);
                    }

                    totalScore += value;
                    scoreByMetric[metric] = (uint)(TotalScoreMultiplier * value);
                }

                var totalScoreUlong = (ulong)(TotalScoreMultiplier * totalScore);
                selectedFactionsByScore.Add(new TempFactionScoreEntry(faction, totalScoreUlong, scoreByMetric));
            }

            selectedFactionsByScore.SortByDesc(f => f.TotalScore);
            for (var index = 0; index < selectedFactionsByScore.Count; index++)
            {
                var entry = selectedFactionsByScore[index];
                var publicState = Faction.GetPublicState(entry.Faction);
                publicState.TotalScore = entry.TotalScore;
                publicState.LeaderboardRank = (ushort)Math.Min(ushort.MaxValue, index + 1);

                var privateState = Faction.GetPrivateState(entry.Faction);
                privateState.LeaderboardScoreByMetric = entry.ScoreByMetric;
            }

            Logger.Important(
                "Factions leaderboard updated. It took "
                + stopwatch.Elapsed.TotalSeconds.ToString("F3")
                + "s."
                + Environment.NewLine
                + "Top 10 factions:"
                + Environment.NewLine
                + selectedFactionsByScore.Take(10)
                                         .Select(tuple =>
                                                     string.Format("* {0} - {1} score points",
                                                                   FactionSystem.SharedGetClanTag(tuple.Faction),
                                                                   tuple.TotalScore))
                                         .GetJoinedString(Environment.NewLine));

            var nextLeaderboardUpdateTime = serverLastUpdateTime + ServerUpdateInterval;
            foreach (var character in Server.Characters.EnumerateAllPlayerCharacters(onlyOnline: true))
            {
                Instance.CallClient(character,
                                    _ => _.ClientRemote_SetNextLeaderboardUpdateTime(nextLeaderboardUpdateTime));
            }
        }

        private void ClientRemote_SetNextLeaderboardUpdateTime(double nextLeaderboardUpdateTime)
        {
            ClientNextLeaderboardUpdateTime = nextLeaderboardUpdateTime;
            Api.SafeInvoke(() => ClientLeaderboardUpdatedHandler?.Invoke());
        }

        private void ServerPlayerOnlineStateChangedHandler(ICharacter character, bool isOnline)
        {
            if (!isOnline)
            {
                return;
            }

            var nextLeaderboardUpdateTime = serverLastUpdateTime + ServerUpdateInterval;
            Instance.CallClient(character,
                                _ => this.ClientRemote_SetNextLeaderboardUpdateTime(nextLeaderboardUpdateTime));
        }

        [RemoteCallSettings(timeInterval: 0.5)]
        [CanBeNull]
        private IReadOnlyDictionary<ProtoFactionScoreMetric, uint> ServerRemote_GetScoreMetrics(string clanTag)
        {
            var faction = FactionSystem.ServerGetFactionByClanTag(clanTag);
            return Faction.GetPrivateState(faction).LeaderboardScoreByMetric;
        }

        [NotPersistent]
        [NotNetworkAvailable]
        private readonly struct TempFactionScoreEntry
        {
            public readonly ILogicObject Faction;

            public readonly IReadOnlyDictionary<ProtoFactionScoreMetric, uint> ScoreByMetric;

            public readonly ulong TotalScore;

            public TempFactionScoreEntry(
                ILogicObject faction,
                ulong totalScore,
                IReadOnlyDictionary<ProtoFactionScoreMetric, uint> scoreByMetric)
            {
                this.Faction = faction;
                this.TotalScore = totalScore;
                this.ScoreByMetric = scoreByMetric;
            }
        }
    }
}