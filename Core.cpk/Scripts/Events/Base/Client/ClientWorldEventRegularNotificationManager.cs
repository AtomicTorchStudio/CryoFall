namespace AtomicTorch.CBND.CoreMod.Events
{
    using System.Collections.Generic;
    using System.Text;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Notifications;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    public static class ClientWorldEventRegularNotificationManager
    {
        public const byte FinishedEventHideDelay = 10;

        private static readonly List<(ILogicObject worldEvent, HudNotificationControl notification)> notifications
            = new();

        static ClientWorldEventRegularNotificationManager()
        {
            RefreshWorldEventsInfo();
        }

        public static int CalculateEventTimeRemains(ILogicObject worldEvent)
        {
            return (int)(worldEvent.GetPublicState<EventPublicState>().EventEndTime
                         - Api.Client.CurrentGame.ServerFrameTimeApproximated);
        }

        public static string GetUpdatedEventNotificationText(
            ILogicObject worldEvent,
            int timeRemains,
            bool addDescription)
        {
            var protoEvent = (IProtoEvent)worldEvent.ProtoGameObject;
            var description = protoEvent.ClientGetDescription(worldEvent);
            if (!protoEvent.ConsolidateNotifications)
            {
                addDescription = true;

                if (worldEvent.IsDestroyed
                    || timeRemains <= 0)
                {
                    return
                        $"{description}[br][br][b]{ClientWorldMapEventVisualizer.Notification_ActiveEvent_Finished}[/b]";
                }
            }

            if (timeRemains < 1)
            {
                timeRemains = 1;
            }

            var sb = new StringBuilder();
            if (addDescription)
            {
                sb.Append(description)
                  .Append("[br][br]");
            }

            if (!worldEvent.IsDestroyed)
            {
                if (worldEvent.ProtoGameObject is ProtoEventBoss protoEventBoss)
                {
                    var timeRemainsToEventStart
                        = protoEventBoss.SharedGetTimeRemainsToEventStart(ProtoEventBoss.GetPublicState(worldEvent));

                    if (timeRemainsToEventStart > 0)
                    {
                        // the boss event is not yet started
                        sb.AppendFormat(ClientWorldMapEventVisualizer.Notification_ActiveEvent_TimeStartsIn,
                                        ClientTimeFormatHelper.FormatTimeDuration(timeRemainsToEventStart));
                    }
                    else
                    {
                        // the boss event has started
                        sb.AppendFormat(ClientWorldMapEventVisualizer.Notification_ActiveEvent_TimeRemainingFormat,
                                        ClientTimeFormatHelper.FormatTimeDuration(timeRemains));
                    }
                }
                else
                {
                    // a regular event
                    sb.AppendFormat(ClientWorldMapEventVisualizer.Notification_ActiveEvent_TimeRemainingFormat,
                                    ClientTimeFormatHelper.FormatTimeDuration(timeRemains));
                }
            }

            var progressText = protoEvent.SharedGetProgressText(worldEvent);
            if (progressText is null)
            {
                return sb.ToString();
            }

            sb.Append("[br]").Append(progressText);
            return sb.ToString();
        }

        public static void RegisterEvent(ILogicObject worldEvent)
        {
            TryCreateNotification(worldEvent);
        }

        public static void UnregisterEvent(ILogicObject worldEvent)
        {
            var notification = RemoveNotification(worldEvent,
                                                  quick: false,
                                                  delaySeconds: FinishedEventHideDelay);
            if (notification is not null)
            {
                notification.Message = GetUpdatedEventNotificationText(worldEvent,
                                                                       CalculateEventTimeRemains(worldEvent),
                                                                       addDescription: false);
            }
        }

        private static HudNotificationControl FindNotification(ILogicObject worldEvent)
        {
            foreach (var pair in notifications)
            {
                if (pair.worldEvent.Equals(worldEvent))
                {
                    return pair.notification;
                }
            }

            return null;
        }

        private static void RefreshWorldEventInfo(ILogicObject worldEvent)
        {
            if (worldEvent.IsDestroyed)
            {
                // the notification will be automatically marked to hide after delay when active event is destroyed
                // (a finished event)
                return;
            }

            var notification = FindNotification(worldEvent);
            if (worldEvent.ProtoGameObject is IProtoEventWithArea { ConsolidateNotifications: true })
            {
                // display the non-consolidated notification only if player is inside the event area
                var publicState = worldEvent.GetPublicState<EventWithAreaPublicState>();
                var playerPosition = ClientCurrentCharacterHelper.Character?.Position
                                     ?? Vector2D.Zero;

                if ((publicState.AreaCirclePosition.ToVector2D() - playerPosition).Length
                    > publicState.AreaCircleRadius)
                {
                    // outside the interaction area
                    notification?.Hide(quick: true);
                }
                else if (notification is null
                         || notification.IsHiding)
                {
                    // inside the interaction area but has no notification displayed
                    if (notification is not null)
                    {
                        RemoveNotification(worldEvent, quick: true);
                    }

                    TryCreateNotification(worldEvent);
                    notification = FindNotification(worldEvent);
                }
            }

            if (notification is null)
            {
                return;
            }

            var timeRemains = CalculateEventTimeRemains(worldEvent);
            if (timeRemains <= 0)
            {
                if (!notification.IsHiding)
                {
                    notification.Message = GetUpdatedEventNotificationText(worldEvent,
                                                                           timeRemains,
                                                                           addDescription: false);
                    RemoveNotification(worldEvent,
                                       quick: false,
                                       delaySeconds: FinishedEventHideDelay);
                }

                return;
            }

            if (!notification.IsHiding)
            {
                notification.Message = GetUpdatedEventNotificationText(worldEvent,
                                                                       timeRemains,
                                                                       addDescription: false);
            }
        }

        private static void RefreshWorldEventsInfo()
        {
            foreach (var pair in Api.Shared.WrapInTempList(notifications).EnumerateAndDispose())
            {
                RefreshWorldEventInfo(pair.worldEvent);
            }

            ClientTimersSystem.AddAction(0.333, RefreshWorldEventsInfo);
        }

        private static HudNotificationControl RemoveNotification(
            ILogicObject worldEvent,
            bool quick,
            double delaySeconds = 0)
        {
            for (var index = 0; index < notifications.Count; index++)
            {
                var pair = notifications[index];
                if (!pair.worldEvent.Equals(worldEvent))
                {
                    continue;
                }

                var notification = pair.notification;
                if (delaySeconds > 0)
                {
                    notification.HideAfterDelay(delaySeconds);
                }
                else
                {
                    notification.Hide(quick);
                }

                notifications.RemoveAt(index--);
                return notification;
            }

            return null;
        }

        private static void TryCreateNotification(ILogicObject worldEvent)
        {
            var timeRemains = CalculateEventTimeRemains(worldEvent);
            if (timeRemains <= 0)
            {
                return;
            }

            var notification = FindNotification(worldEvent);
            if (notification is not null)
            {
                // notification already exist
                return;
            }

            // notify player about the new event
            var protoEvent = (IProtoEvent)worldEvent.ProtoGameObject;
            notification = NotificationSystem.ClientShowNotification(
                ClientWorldMapEventVisualizer.Notification_ActiveEvent_Title,
                GetUpdatedEventNotificationText(worldEvent,
                                                timeRemains,
                                                addDescription: false),
                icon: protoEvent.Icon,
                autoHide: false,
                playSound: false,
                color: NotificationColor.Event);

            if (!protoEvent.ConsolidateNotifications)
            {
                ClientEventSoundHelper.PlayEventStartedSound();
            }

            RemoveNotification(worldEvent, true);
            notifications.Add((worldEvent, notification));
            RefreshWorldEventInfo(worldEvent);
        }
    }
}