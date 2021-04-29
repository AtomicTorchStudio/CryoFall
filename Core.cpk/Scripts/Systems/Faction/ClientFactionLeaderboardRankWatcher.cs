namespace AtomicTorch.CBND.CoreMod.Systems.Faction
{
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;

    /// <summary>
    /// This watcher displays a notification when the player's faction raises or drops in the leaderboard.
    /// Applies only to private factions that have accumulated a decent faction score overall
    /// (to prevent unuseful notifications spam).
    /// </summary>
    public class ClientFactionLeaderboardRankWatcher
    {
        public const ulong MinScoreToDisplayNotifications = 200;

        public const string Notification_RankDecreased_Message_Format
            = "Your faction has just fallen from #{0} to #{1} in the leaderboard.";

        public const string Notification_RankDecreased_Title = "Faction rank decreased";

        public const string Notification_RankIncreased_Message_Format
            = @"Your faction has risen from #{0} to #{1} in the leaderboard.
  [br]Keep it up!";

        public const string Notification_RankIncreased_Title = "Faction rank increased";

        private static readonly StateSubscriptionStorage subscriptionStorage = new();

        private static FactionPublicState factionPublicState;

        private static ushort lastRank;

        private static void ClientLeaderboardRankChangedHandler()
        {
            var oldRank = lastRank;
            var newRank = factionPublicState.LeaderboardRank;

            if (oldRank == newRank
                || oldRank == 0
                || newRank == 0)
            {
                return;
            }

            lastRank = newRank;
            if (factionPublicState.TotalScore < MinScoreToDisplayNotifications)
            {
                return;
            }

            var isRankIncreased = newRank < oldRank;
            var title = isRankIncreased
                            ? Notification_RankIncreased_Title
                            : Notification_RankDecreased_Title;

            var messageFormat = isRankIncreased
                                    ? Notification_RankIncreased_Message_Format
                                    : Notification_RankDecreased_Message_Format;

            var message = string.Format(messageFormat, oldRank, newRank);

            NotificationSystem.ClientShowNotification(
                title,
                message,
                color: isRankIncreased
                           ? NotificationColor.Good
                           : NotificationColor.Neutral,
                icon: ClientFactionEmblemTextureProvider.GetEmblemTexture(FactionSystem.ClientCurrentFaction,
                                                                          useCache: true));
        }

        private static void CurrentFactionChangedHandler()
        {
            SetCurrentFaction(FactionSystem.ClientCurrentFaction);
        }

        private static void SetCurrentFaction(ILogicObject faction)
        {
            subscriptionStorage.ReleaseSubscriptions();

            if (faction is null)
            {
                factionPublicState = null;
                return;
            }

            factionPublicState = Faction.GetPublicState(faction);
            if (factionPublicState.Kind == FactionKind.Public)
            {
                return;
            }

            lastRank = factionPublicState.LeaderboardRank;
            factionPublicState.ClientSubscribe(_ => _.LeaderboardRank,
                                               _ => ClientLeaderboardRankChangedHandler(),
                                               subscriptionStorage);
        }

        private class Bootstrapper : BaseBootstrapper
        {
            public override void ClientInitialize()
            {
                FactionSystem.ClientCurrentFactionChanged += CurrentFactionChangedHandler;
                SetCurrentFaction(FactionSystem.ClientCurrentFaction);
            }
        }
    }
}