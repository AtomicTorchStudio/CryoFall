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
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ClientWorldMapEventVisualizer : IWorldMapVisualizer
    {
        public const string Notification_ActiveEvent_Finished = "Event finished!";

        public const string Notification_ActiveEvent_TimeRemainingFormat = "Time remaining: {0}";

        public const string Notification_ActiveEvent_Title = "Active event";

        private const byte FinishedEventHideDelay = 10;

        private static readonly IWorldClientService ClientWorld = Api.Client.World;

        private readonly bool enableNotifications;

        private readonly List<(ILogicObject activeEvent, HUDNotificationControl notification)> notifications
            = new List<(ILogicObject activeEvent, HUDNotificationControl notification)>();

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
            var text = GetUpdatedRecentResourceNotificationText(activeEvent, timeRemains);
            return $"[b]{Notification_ActiveEvent_Title}[/b][br]{text}";
        }

        private static string GetUpdatedRecentResourceNotificationText(
            ILogicObject activeEvent,
            int timeRemains)
        {
            var protoEvent = (IProtoEvent)activeEvent.ProtoGameObject;
            if (activeEvent.IsDestroyed)
            {
                return $"{protoEvent.Description}[br][br][b]{Notification_ActiveEvent_Finished}[/b]";
            }

            if (timeRemains < 1)
            {
                timeRemains = 1;
            }

            var sb = new StringBuilder().Append(protoEvent.Description)
                                        .Append("[br][br]")
                                        .AppendFormat(Notification_ActiveEvent_TimeRemainingFormat,
                                                      ClientTimeFormatHelper.FormatTimeDuration(timeRemains));

            var progressText = protoEvent.SharedGetProgressText(activeEvent);
            if (progressText is null)
            {
                return sb.ToString();
            }

            sb.Append("[br]").Append(progressText);
            return sb.ToString();
        }

        private void AddNotification(ILogicObject activeEvent, HUDNotificationControl notification)
        {
            this.RemoveNotification(activeEvent, quick: true);
            this.notifications.Add((activeEvent, notification));
        }

        private HUDNotificationControl FindNotification(ILogicObject activeEvent)
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

            // add a circle for the search area
            var publicState = activeEvent.GetPublicState<EventWithAreaPublicState>();
            var circleRadius = publicState.AreaCircleRadius;
            var circleCanvasPosition = this.worldMapController.WorldToCanvasPosition(
                publicState.AreaCirclePosition.ToVector2D());

            var control = new WorldMapMarkEvent
            {
                Width = 2 * circleRadius * WorldMapTexturesProvider.WorldTileTextureSize,
                Height = 2 * circleRadius * WorldMapTexturesProvider.WorldTileTextureSize,
                EllipseColorStroke = Color.FromArgb(0x99, 0x77, 0xBB, 0xFF),
                EllipseColorStart = Color.FromArgb(0x00,  0x66, 0x99, 0xCC),
                EllipseColorEnd = Color.FromArgb(0x55,    0x66, 0x99, 0xCC)
            };

            Canvas.SetLeft(control, circleCanvasPosition.X - control.Width / 2);
            Canvas.SetTop(control, circleCanvasPosition.Y - control.Height / 2);
            Panel.SetZIndex(control, 1);
            this.worldMapController.AddControl(control, scaleWithZoom: false);
            this.visualizedSearchAreas.Add((activeEvent, control));
            ToolTipServiceExtend.SetToolTip(
                control,
                new WorldMapMarkEventTooltip()
                {
                    Text = GetTooltipText(activeEvent),
                    Icon = Api.Client.UI.GetTextureBrush(
                        ((IProtoEvent)activeEvent.ProtoGameObject).Icon)
                });

            this.TryCreateNotification(activeEvent);

            // ensure the map mark would be removed after the timeout
            ClientTimersSystem.AddAction(timeRemains + 1,
                                         () => this.OnActiveEventRemoved(activeEvent));
        }

        private void OnActiveEventRemoved(ILogicObject activeEvent)
        {
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
                                                       quick: false,
                                                       delaySeconds: FinishedEventHideDelay);
            notification?.SetMessage(
                GetUpdatedRecentResourceNotificationText(activeEvent,
                                                         CalculateEventTimeRemains(activeEvent)));
        }

        private HUDNotificationControl RemoveNotification(
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
            if (!(notification is null))
            {
                // notification already exist
                return;
            }

            // notify player about the new resource
            notification = NotificationSystem.ClientShowNotification(
                title: Notification_ActiveEvent_Title,
                message: GetUpdatedRecentResourceNotificationText(activeEvent, timeRemains),
                icon: ((IProtoEvent)activeEvent.ProtoGameObject).Icon,
                autoHide: false);

            this.RemoveNotification(activeEvent, quick: true);
            this.notifications.Add((activeEvent, notification));
            this.UpdateNotification(activeEvent, notification);
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

        private void UpdateNotification(
            ILogicObject activeEvent,
            HUDNotificationControl notification)
        {
            if (activeEvent.IsDestroyed)
            {
                // the notification will be automatically marked to hide after delay when active event is destroyed
                // (a finished event)
                return;
            }

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
                notification.SetMessage(
                    GetUpdatedRecentResourceNotificationText(activeEvent, timeRemains));
            }

            this.UpdateEventTooltip(activeEvent);

            // schedule recursive update in a second
            ClientTimersSystem.AddAction(
                delaySeconds: 1,
                () => this.UpdateNotification(activeEvent, notification));
        }
    }
}