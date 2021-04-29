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

        private static readonly List<(ILogicObject activeEvent, HudNotificationControl notification)> notifications
            = new();

        static ClientWorldEventRegularNotificationManager()
        {
            RefreshActiveEventsInfo();
        }

        public static int CalculateEventTimeRemains(ILogicObject activeEvent)
        {
            return (int)(activeEvent.GetPublicState<EventPublicState>().EventEndTime
                         - Api.Client.CurrentGame.ServerFrameTimeApproximated);
        }

        public static string GetUpdatedEventNotificationText(
            ILogicObject activeEvent,
            int timeRemains,
            bool addDescription)
        {
            var protoEvent = (IProtoEvent)activeEvent.ProtoGameObject;
            var description = protoEvent.ClientGetDescription(activeEvent);
            if (!protoEvent.ConsolidateNotifications)
            {
                addDescription = true;

                if (activeEvent.IsDestroyed
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

            if (!activeEvent.IsDestroyed)
            {
                if (activeEvent.ProtoGameObject is ProtoEventBoss protoEventBoss)
                {
                    var timeRemainsToEventStart
                        = protoEventBoss.SharedGetTimeRemainsToEventStart(ProtoEventBoss.GetPublicState(activeEvent));

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

            var progressText = protoEvent.SharedGetProgressText(activeEvent);
            if (progressText is null)
            {
                return sb.ToString();
            }

            sb.Append("[br]").Append(progressText);
            return sb.ToString();
        }

        public static void RegisterEvent(ILogicObject activeEvent)
        {
            TryCreateNotification(activeEvent);
        }

        public static void UnregisterEvent(ILogicObject activeEvent)
        {
            var notification = RemoveNotification(activeEvent,
                                                  quick: false,
                                                  delaySeconds: FinishedEventHideDelay);
            if (notification is not null)
            {
                notification.Message = GetUpdatedEventNotificationText(activeEvent,
                                                                       CalculateEventTimeRemains(activeEvent),
                                                                       addDescription: false);
            }
        }

        private static HudNotificationControl FindNotification(ILogicObject activeEvent)
        {
            foreach (var pair in notifications)
            {
                if (pair.activeEvent.Equals(activeEvent))
                {
                    return pair.notification;
                }
            }

            return null;
        }

        private static void RefreshActiveEventInfo(ILogicObject activeEvent)
        {
            if (activeEvent.IsDestroyed)
            {
                // the notification will be automatically marked to hide after delay when active event is destroyed
                // (a finished event)
                return;
            }

            var notification = FindNotification(activeEvent);
            if (activeEvent.ProtoGameObject is IProtoEventWithArea { ConsolidateNotifications: true })
            {
                // display the non-consolidated notification only if player is inside the event area
                var publicState = activeEvent.GetPublicState<EventWithAreaPublicState>();
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
                        RemoveNotification(activeEvent, quick: true);
                    }

                    TryCreateNotification(activeEvent);
                    notification = FindNotification(activeEvent);
                }
            }

            if (notification is null)
            {
                return;
            }

            var timeRemains = CalculateEventTimeRemains(activeEvent);
            if (timeRemains <= 0)
            {
                if (!notification.IsHiding)
                {
                    notification.Message = GetUpdatedEventNotificationText(activeEvent,
                                                                           timeRemains,
                                                                           addDescription: false);
                    RemoveNotification(activeEvent,
                                       quick: false,
                                       delaySeconds: FinishedEventHideDelay);
                }

                return;
            }

            if (!notification.IsHiding)
            {
                notification.Message = GetUpdatedEventNotificationText(activeEvent,
                                                                       timeRemains,
                                                                       addDescription: false);
            }
        }

        private static void RefreshActiveEventsInfo()
        {
            foreach (var pair in Api.Shared.WrapInTempList(notifications).EnumerateAndDispose())
            {
                RefreshActiveEventInfo(pair.activeEvent);
            }

            ClientTimersSystem.AddAction(0.333, RefreshActiveEventsInfo);
        }

        private static HudNotificationControl RemoveNotification(
            ILogicObject activeEvent,
            bool quick,
            double delaySeconds = 0)
        {
            for (var index = 0; index < notifications.Count; index++)
            {
                var pair = notifications[index];
                if (!pair.activeEvent.Equals(activeEvent))
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

        private static void TryCreateNotification(ILogicObject activeEvent)
        {
            var timeRemains = CalculateEventTimeRemains(activeEvent);
            if (timeRemains <= 0)
            {
                return;
            }

            var notification = FindNotification(activeEvent);
            if (notification is not null)
            {
                // notification already exist
                return;
            }

            // notify player about the new event
            var protoEvent = (IProtoEvent)activeEvent.ProtoGameObject;
            notification = NotificationSystem.ClientShowNotification(
                ClientWorldMapEventVisualizer.Notification_ActiveEvent_Title,
                GetUpdatedEventNotificationText(activeEvent,
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

            RemoveNotification(activeEvent, true);
            notifications.Add((activeEvent, notification));
            RefreshActiveEventInfo(activeEvent);
        }
    }
}