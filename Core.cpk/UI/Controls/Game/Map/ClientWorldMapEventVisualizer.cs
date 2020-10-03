namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Events.Base;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Notifications;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map.Data;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ClientWorldMapEventVisualizer : IWorldMapVisualizer
    {
        public const byte FinishedEventHideDelay = 10;

        public const string Notification_ActiveEvent_Finished = "Event finished!";

        public const string Notification_ActiveEvent_TimeRemainingFormat = "Time remaining: {0}";

        public const string Notification_ActiveEvent_Title = "Active event";

        private static readonly IWorldClientService ClientWorld = Api.Client.World;

        private readonly bool enableNotifications;

        private readonly List<(ILogicObject activeEvent, HudNotificationControl notification)> notifications
            = new List<(ILogicObject activeEvent, HudNotificationControl notification)>();

        private readonly List<(ILogicObject activeEvent, FrameworkElement mapControl)> visualizedSearchAreas
            = new List<(ILogicObject activeEvent, FrameworkElement mapControl)>();

        private readonly WorldMapController worldMapController;

        public ClientWorldMapEventVisualizer(
            WorldMapController worldMapController,
            bool enableNotifications)
        {
            this.worldMapController = worldMapController;
            this.enableNotifications = enableNotifications;

            ClientWorld.LogicObjectInitialized += this.LogicObjectInitializedHandler;
            ClientWorld.LogicObjectDeinitialized += this.LogicObjectDeinitializedHandler;

            foreach (var activeEvent in ClientWorld.GetGameObjectsOfProto<ILogicObject, IProtoEventWithArea>())
            {
                this.OnActiveEventAdded(activeEvent);
            }
        }

        public bool IsEnabled { get; set; }

        public void Dispose()
        {
            ClientWorld.LogicObjectInitialized -= this.LogicObjectInitializedHandler;
            ClientWorld.LogicObjectDeinitialized -= this.LogicObjectDeinitializedHandler;

            if (this.visualizedSearchAreas.Count > 0)
            {
                foreach (var visualizedArea in this.visualizedSearchAreas.ToList())
                {
                    this.OnActiveEventRemoved(visualizedArea.activeEvent);
                }
            }
        }

        private static int CalculateEventTimeRemains(ILogicObject activeEvent)
        {
            return (int)(activeEvent.GetPublicState<EventWithAreaPublicState>().EventEndTime
                         - Api.Client.CurrentGame.ServerFrameTimeApproximated);
        }

        private static string GetTooltipText(ILogicObject activeEvent)
        {
            var timeRemains = CalculateEventTimeRemains(activeEvent);
            var text = GetUpdatedEventNotificationText(activeEvent,
                                                       timeRemains,
                                                       addDescription: true);
            return $"[b]{Notification_ActiveEvent_Title}[/b][br]{text}";
        }

        private static string GetUpdatedEventNotificationText(
            ILogicObject activeEvent,
            int timeRemains,
            bool addDescription)
        {
            var protoEvent = (IProtoEvent)activeEvent.ProtoGameObject;
            if (!protoEvent.ConsolidateNotifications)
            {
                addDescription = true;
            }

            if (activeEvent.IsDestroyed)
            {
                return addDescription
                           ? $"[b]{Notification_ActiveEvent_Finished}[/b]"
                           : $"{protoEvent.Description}[br][br][b]{Notification_ActiveEvent_Finished}[/b]";
            }

            if (timeRemains < 1)
            {
                timeRemains = 1;
            }

            var sb = new StringBuilder();
            if (addDescription)
            {
                sb.Append(protoEvent.Description)
                  .Append("[br][br]");
            }

            sb.AppendFormat(Notification_ActiveEvent_TimeRemainingFormat,
                            ClientTimeFormatHelper.FormatTimeDuration(timeRemains));

            var progressText = protoEvent.SharedGetProgressText(activeEvent);
            if (progressText is null)
            {
                return sb.ToString();
            }

            sb.Append("[br]").Append(progressText);
            return sb.ToString();
        }

        private HudNotificationControl FindNotification(ILogicObject activeEvent)
        {
            foreach (var pair in this.notifications)
            {
                if (pair.activeEvent.Equals(activeEvent))
                {
                    return pair.notification;
                }
            }

            return null;
        }

        private void LogicObjectDeinitializedHandler(ILogicObject obj)
        {
            if (obj.ProtoGameObject is IProtoEvent)
            {
                this.OnActiveEventRemoved(obj);
            }
        }

        private void LogicObjectInitializedHandler(ILogicObject obj)
        {
            if (obj.ProtoGameObject is IProtoEvent)
            {
                this.OnActiveEventAdded(obj);
            }
        }

        private void OnActiveEventAdded(ILogicObject activeEvent)
        {
            var timeRemains = CalculateEventTimeRemains(activeEvent);
            if (timeRemains < 10)
            {
                // less than 10 seconds remains - not worth to display it on the map
                return;
            }

            if (activeEvent.ProtoGameObject is IProtoEventWithArea)
            {
                // add a circle for the search area
                var publicState = activeEvent.GetPublicState<EventWithAreaPublicState>();
                var circleRadius = publicState.AreaCircleRadius;
                var circleCanvasPosition = this.worldMapController.WorldToCanvasPosition(
                    publicState.AreaCirclePosition.ToVector2D());

                var control = new WorldMapMarkEvent
                {
                    Width = 2 * circleRadius * WorldMapTexturesProvider.WorldTileTextureSize,
                    Height = 2 * circleRadius * WorldMapTexturesProvider.WorldTileTextureSize,
                    EllipseColorStroke = Color.FromArgb(0xDD, 0x66, 0xAA, 0xEE),
                    EllipseColorStart = Color.FromArgb(0x00,  0x66, 0xAA, 0xEE),
                    EllipseColorEnd = Color.FromArgb(0x77,    0x66, 0xAA, 0xEE)
                };

                Canvas.SetLeft(control, circleCanvasPosition.X - control.Width / 2);
                Canvas.SetTop(control, circleCanvasPosition.Y - control.Height / 2);
                Panel.SetZIndex(control, 1);
                this.worldMapController.AddControl(control, false);
                this.visualizedSearchAreas.Add((activeEvent, control));
                ToolTipServiceExtend.SetToolTip(
                    control,
                    new WorldMapMarkEventTooltip()
                    {
                        Text = GetTooltipText(activeEvent),
                        Icon = Api.Client.UI.GetTextureBrush(
                            ((IProtoEvent)activeEvent.ProtoGameObject).Icon)
                    });
            }

            ClientWorldMapEventVisualizerNotificationManager.RegisterEvent(activeEvent);
            this.TryCreateNotification(activeEvent);

            // ensure the map mark would be removed after the timeout
            ClientTimersSystem.AddAction(timeRemains + 1,
                                         () => this.OnActiveEventRemoved(activeEvent));
        }

        private void OnActiveEventRemoved(ILogicObject activeEvent)
        {
            ClientWorldMapEventVisualizerNotificationManager.UnregisterEvent(activeEvent);

            for (var index = 0; index < this.visualizedSearchAreas.Count; index++)
            {
                var entry = this.visualizedSearchAreas[index];
                if (!entry.activeEvent.Equals(activeEvent))
                {
                    continue;
                }

                this.visualizedSearchAreas.RemoveAt(index);
                this.worldMapController.RemoveControl(entry.mapControl);
            }

            var notification = this.RemoveNotification(activeEvent,
                                                       false,
                                                       FinishedEventHideDelay);
            if (notification is not null)
            {
                notification.Message = GetUpdatedEventNotificationText(activeEvent,
                                                                       CalculateEventTimeRemains(activeEvent),
                                                                       addDescription: false);
            }
        }

        private void RefreshActiveEventInfo(ILogicObject activeEvent)
        {
            if (activeEvent.IsDestroyed)
            {
                // the notification will be automatically marked to hide after delay when active event is destroyed
                // (a finished event)
                return;
            }

            var notification = this.FindNotification(activeEvent);
            if (activeEvent.ProtoGameObject is IProtoEventWithArea protoEventWithArea
                && protoEventWithArea.ConsolidateNotifications)
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
                        this.RemoveNotification(activeEvent, quick: true);
                    }

                    this.TryCreateNotification(activeEvent);
                    notification = this.FindNotification(activeEvent);
                }
            }

            if (notification is not null)
            {
                var timeRemains = CalculateEventTimeRemains(activeEvent);
                if (timeRemains <= 0)
                {
                    if (!notification.IsHiding)
                    {
                        this.RemoveNotification(activeEvent, quick: false);
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

            this.UpdateEventTooltip(activeEvent);

            // schedule recursive update in a second
            ClientTimersSystem.AddAction(
                1,
                () => this.RefreshActiveEventInfo(activeEvent));
        }

        private HudNotificationControl RemoveNotification(
            ILogicObject activeEvent,
            bool quick,
            double delaySeconds = 0)
        {
            for (var index = 0; index < this.notifications.Count; index++)
            {
                var pair = this.notifications[index];
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

                this.notifications.RemoveAt(index--);
                return notification;
            }

            return null;
        }

        private void TryCreateNotification(ILogicObject activeEvent)
        {
            if (!this.enableNotifications)
            {
                return;
            }

            var timeRemains = CalculateEventTimeRemains(activeEvent);
            if (timeRemains <= 0)
            {
                return;
            }

            var notification = this.FindNotification(activeEvent);
            if (notification is not null)
            {
                // notification already exist
                return;
            }

            // notify player about the new event
            var protoEvent = (IProtoEvent)activeEvent.ProtoGameObject;
            notification = NotificationSystem.ClientShowNotification(
                Notification_ActiveEvent_Title,
                GetUpdatedEventNotificationText(activeEvent,
                                                timeRemains,
                                                addDescription: false),
                icon: protoEvent.Icon,
                autoHide: false,
                playSound: false);
            
            if (!protoEvent.ConsolidateNotifications)
            {
                ClientEventSoundHelper.PlayEventStartedSound();
            }

            this.RemoveNotification(activeEvent, true);
            this.notifications.Add((activeEvent, notification));
            this.RefreshActiveEventInfo(activeEvent);
        }

        private void UpdateEventTooltip(ILogicObject activeEvent)
        {
            foreach (var entry in this.visualizedSearchAreas)
            {
                if (!entry.activeEvent.Equals(activeEvent))
                {
                    continue;
                }

                var control = entry.mapControl;
                var formattedTextBlock = (WorldMapMarkEventTooltip)ToolTipServiceExtend.GetToolTip(control);
                formattedTextBlock.Text = GetTooltipText(activeEvent);
            }
        }
    }
}