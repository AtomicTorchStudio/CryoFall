namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map
{
    using System.Collections.Generic;
    using System.Text;
    using AtomicTorch.CBND.CoreMod.Events.Base;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Notifications;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;

    public static class ClientWorldMapEventVisualizerNotificationManager
    {
        private static readonly Dictionary<IProtoEvent, ConsolidatedEventNotification> EventNotifications
            = new Dictionary<IProtoEvent, ConsolidatedEventNotification>();

        public static void RegisterEvent(ILogicObject activeEvent)
        {
            var protoEvent = (IProtoEvent)activeEvent.ProtoGameObject;
            if (!protoEvent.ConsolidateNotifications)
            {
                return;
            }

            if (!EventNotifications.TryGetValue(protoEvent, out var entry))
            {
                entry = new ConsolidatedEventNotification(protoEvent);
                EventNotifications[protoEvent] = entry;
            }

            entry.AddEvent(activeEvent);
        }

        public static void UnregisterEvent(ILogicObject activeEvent)
        {
            var protoEvent = (IProtoEvent)activeEvent.ProtoGameObject;
            if (!protoEvent.ConsolidateNotifications)
            {
                return;
            }

            if (!EventNotifications.TryGetValue(protoEvent, out var entry))
            {
                return;
            }

            entry.RemoveEvent(activeEvent);
            if (entry.IsDestroyed)
            {
                EventNotifications.Remove(protoEvent);
            }
        }

        private class ConsolidatedEventNotification
        {
            public readonly IProtoEvent ProtoEvent;

            private readonly List<ILogicObject> activeEvents = new List<ILogicObject>();

            private HudNotificationControl notification;

            public ConsolidatedEventNotification(IProtoEvent protoEvent)
            {
                this.ProtoEvent = protoEvent;
            }

            public bool IsDestroyed { get; private set; }

            public void AddEvent(ILogicObject activeEvent)
            {
                if (this.IsDestroyed)
                {
                    Api.Logger.Error("The consolidated event notification is destroyed");
                }

                this.activeEvents.Add(activeEvent);
                this.TryCreateNotification();
            }

            public void RemoveEvent(ILogicObject activeEvent)
            {
                this.activeEvents.Remove(activeEvent);
                if (this.activeEvents.Count != 0)
                {
                    return;
                }

                this.RemoveNotification(quick: false);
                this.IsDestroyed = true;
            }

            private int CalculateEventTimeRemains()
            {
                var maxTimeRemains = 0;
                var serverTime = Api.Client.CurrentGame.ServerFrameTimeApproximated;
                foreach (var activeEvent in this.activeEvents)
                {
                    var eventState = activeEvent.GetPublicState<EventWithAreaPublicState>();
                    var timeRemains = (int)(eventState.EventEndTime - serverTime);
                    if (timeRemains > maxTimeRemains)
                    {
                        maxTimeRemains = timeRemains;
                    }
                }

                return maxTimeRemains;
            }

            private string GetUpdatedEventNotificationText(int timeRemains)
            {
                if (this.activeEvents.Count == 0
                    || this.activeEvents.TrueForAll(e => e.IsDestroyed)
                    || timeRemains < 1)
                {
                    return string.Format("{0}[br][br][b]{1}[/b]",
                                         this.ProtoEvent.Description,
                                         ClientWorldMapEventVisualizer.Notification_ActiveEvent_Finished);
                }

                var sb = new StringBuilder(this.ProtoEvent.Description)
                    .Append("[br][br]");

                sb.AppendFormat(ClientWorldMapEventVisualizer.Notification_ActiveEvent_TimeRemainingFormat,
                                ClientTimeFormatHelper.FormatTimeDuration(timeRemains));

                return sb.ToString();
            }

            private void RefreshNotification()
            {
                if (this.notification is null
                    || this.IsDestroyed
                    || this.activeEvents.TrueForAll(e => e.IsDestroyed))
                {
                    // the notification will be automatically marked to hide after delay when active event is destroyed
                    // (a finished event)
                    return;
                }

                var timeRemains = this.CalculateEventTimeRemains();
                if (timeRemains <= 0)
                {
                    if (!this.notification.IsHiding)
                    {
                        this.RemoveNotification(quick: false);
                    }

                    return;
                }

                this.notification.Message = this.GetUpdatedEventNotificationText(timeRemains);

                // schedule recursive update in a second
                ClientTimersSystem.AddAction(1, this.RefreshNotification);
            }

            private void RemoveNotification(bool quick)
            {
                this.notification.Message = this.GetUpdatedEventNotificationText(
                    this.CalculateEventTimeRemains());

                if (quick)
                {
                    this.notification.Hide(quick: true);
                }
                else
                {
                    var delaySeconds = ClientWorldMapEventVisualizer.FinishedEventHideDelay;
                    this.notification.HideAfterDelay(delaySeconds);
                }
            }

            private void TryCreateNotification()
            {
                if (this.notification != null)
                {
                    return;
                }

                var timeRemains = this.CalculateEventTimeRemains();
                if (timeRemains <= 0)
                {
                    return;
                }

                if (!(this.notification is null))
                {
                    // notification already exist
                    return;
                }

                // notify player about the new event
                this.notification = NotificationSystem.ClientShowNotification(
                    ClientWorldMapEventVisualizer.Notification_ActiveEvent_Title,
                    this.GetUpdatedEventNotificationText(timeRemains),
                    icon: this.ProtoEvent.Icon,
                    autoHide: false,
                    playSound: false);
                this.RefreshNotification();

                ClientEventSoundHelper.PlayEventStartedSound();
            }
        }
    }
}