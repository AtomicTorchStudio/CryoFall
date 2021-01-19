namespace AtomicTorch.CBND.CoreMod.Systems.Faction
{
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;

    public static class ClientFactionEventsLogListener
    {
        private static NetworkSyncList<BaseFactionEventLogEntry> recentEventsLog;

        public static void Reset()
        {
            if (recentEventsLog is null)
            {
                return;
            }

            recentEventsLog.ClientElementInserted -= LogEntryAdded;
            recentEventsLog = null;
        }

        public static void Setup(ILogicObject faction)
        {
            Reset();

            if (faction is null)
            {
                return;
            }

            recentEventsLog = Faction.GetPrivateState(faction).RecentEventsLog;
            recentEventsLog.ClientElementInserted += LogEntryAdded;
        }

        private static void LogEntryAdded(
            NetworkSyncList<BaseFactionEventLogEntry> source,
            int index,
            BaseFactionEventLogEntry entry)
        {
            if (entry.ClientShowNotification)
            {
                var message = entry.ClientText;
                if (!string.IsNullOrEmpty(entry.ByMemberName))
                {
                    message += "[br]"
                               + string.Format(BaseFactionEventLogEntry.Text_Name_Format, entry.ByMemberName);
                }

                var notification = NotificationSystem.ClientShowNotification(CoreStrings.Faction_Title,
                                                                             message,
                                                                             entry.ClientNotificationColor,
                                                                             icon: entry.IconResource);

                if (entry.ClientIsLongNotification)
                {
                    notification.HideAfterDelay(60);
                }
            }

            entry.ClientOnReceived();
        }
    }
}