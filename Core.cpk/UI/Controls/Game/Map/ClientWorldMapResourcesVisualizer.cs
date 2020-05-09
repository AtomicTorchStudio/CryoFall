namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Perks;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Deposits;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.WorldMapResourceMarks;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Notifications;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ClientWorldMapResourcesVisualizer : IWorldMapVisualizer
    {
        // {0} is the object name (Oil seep or Geothermal spring), {1} and {2} are coordinate numbers, {3} is the time remaining
        public const string Notification_NewResourceAvailable_MessageFormat =
            @"{0} located at {1};{2}.
              [br]It will be available for claiming after:
              [br]{3}.
              [br]Grab your guns and get ready to capture it!";

        public const string Notification_NewResourceAvailable_Title =
            "New resource available";

        public const string TooltipDepositSearchAreaFormat = "Search area: {0}";

        private readonly bool drawSearchAreas;

        private readonly bool enableNotifications;

        private readonly List<(WorldMapResourceMark mark, HUDNotificationControl notification)> notifications
            = new List<(WorldMapResourceMark mark, HUDNotificationControl notification)>();

        private readonly List<(WorldMapResourceMark mark, FrameworkElement mapControl)> visualizedMarks
            = new List<(WorldMapResourceMark, FrameworkElement)>();

        private readonly List<(WorldMapResourceMark mark, FrameworkElement mapControl)> visualizedSearchAreas
            = new List<(WorldMapResourceMark, FrameworkElement)>();

        private readonly WorldMapController worldMapController;

        public ClientWorldMapResourcesVisualizer(
            WorldMapController worldMapController,
            bool enableNotifications,
            bool drawSearchAreas = true)
        {
            this.worldMapController = worldMapController;
            this.enableNotifications = enableNotifications;
            this.drawSearchAreas = drawSearchAreas;

            WorldMapResourceMarksSystem.ClientMarkAdded += this.MarkAddedHandler;
            WorldMapResourceMarksSystem.ClientMarkRemoved += this.MarkRemovedHandler;
            WorldMapResourceMarksSystem.ClientDepositClaimCooldownDurationReceived +=
                this.MarkDepositClaimCooldownDurationReceivedHandler;

            foreach (var mark in WorldMapResourceMarksSystem.SharedEnumerateMarks())
            {
                this.MarkAddedHandler(mark);
            }
        }

        public bool IsEnabled { get; set; }

        public void Dispose()
        {
            WorldMapResourceMarksSystem.ClientMarkAdded -= this.MarkAddedHandler;
            WorldMapResourceMarksSystem.ClientMarkRemoved -= this.MarkRemovedHandler;
            WorldMapResourceMarksSystem.ClientDepositClaimCooldownDurationReceived -=
                this.MarkDepositClaimCooldownDurationReceivedHandler;

            if (this.visualizedMarks.Count > 0)
            {
                foreach (var visualizedArea in this.visualizedMarks.ToList())
                {
                    this.MarkRemovedHandler(visualizedArea.mark);
                }
            }
        }

        private static FrameworkElement GetMapControl(WorldMapResourceMark mark)
        {
            switch (mark.ProtoWorldObject)
            {
                case ObjectDepositOilSeep _:
                    return new WorldMapMarkResourceOil()
                        { IsInfiniteSource = mark.ProtoWorldObject is ObjectDepositOilSeepInfinite };

                case ObjectDepositGeothermalSpring _:
                    return new WorldMapMarkResourceLithium()
                        { IsInfiniteSource = mark.ProtoWorldObject is ObjectDepositGeothermalSpringInfinite };

                default:
                    return null;
            }
        }

        private static string GetUpdatedRecentResourceNotificationText(
            WorldMapResourceMark mark,
            int timeRemains)
        {
            if (timeRemains < 1)
            {
                timeRemains = 1;
            }

            var protoResource = mark.ProtoWorldObject;
            if (mark.Position == default)
            {
                // write biome name instead
                return string.Format(Notification_NewResourceAvailable_MessageFormat.Replace("{1};{2}", "[br]{1}{2}"),
                                     protoResource.Name,
                                     mark.Biome.Name,
                                     string.Empty,
                                     ClientTimeFormatHelper.FormatTimeDuration(timeRemains));
            }

            var localPosition = mark.Position - Api.Client.World.WorldBounds.Offset;
            return string.Format(Notification_NewResourceAvailable_MessageFormat,
                                 protoResource.Name,
                                 localPosition.X,
                                 localPosition.Y,
                                 ClientTimeFormatHelper.FormatTimeDuration(timeRemains));
        }

        private void AddNotification(in WorldMapResourceMark mark, HUDNotificationControl notification)
        {
            this.RemoveNotification(mark, quick: true);
            this.notifications.Add((mark, notification));
        }

        private HUDNotificationControl FindNotification(in WorldMapResourceMark mark)
        {
            foreach (var pair in this.notifications)
            {
                if (pair.mark.Equals(mark))
                {
                    return pair.notification;
                }
            }

            return null;
        }

        private void MarkAddedHandler(WorldMapResourceMark mark)
        {
            var mapControl = GetMapControl(mark);

            if (mapControl == null)
            {
                Api.Logger.Warning("Unknown world object mark: "
                                   + mark.ProtoWorldObject
                                   + " - there is no UI control for this world object prototype");
                return;
            }

            var canvasPosition = this.worldMapController.WorldToCanvasPosition(mark.Position.ToVector2D());
            Canvas.SetLeft(mapControl, canvasPosition.X);
            Canvas.SetTop(mapControl, canvasPosition.Y);
            Panel.SetZIndex(mapControl, 16);

            this.worldMapController.AddControl(mapControl);
            this.visualizedMarks.Add((mark, mapControl));

            if (mark.SearchAreaCirclePosition != default
                && this.drawSearchAreas)
            {
                // add a circle for the search area
                var circleRadius = mark.SearchAreaCircleRadius;
                var control = new WorldMapMarkEvent
                {
                    Width = 2 * circleRadius * WorldMapTexturesProvider.WorldTileTextureSize,
                    Height = 2 * circleRadius * WorldMapTexturesProvider.WorldTileTextureSize,
                    EllipseColorStroke = Color.FromArgb(0xDD, 0xCC, 0x66, 0x66),
                    EllipseColorStart = Color.FromArgb(0x00, 0xCC, 0x66, 0x66),
                    EllipseColorEnd = Color.FromArgb(0x77,   0xCC, 0x66, 0x66)
                };

                var circleCanvasPosition = this.worldMapController.WorldToCanvasPosition(
                    mark.SearchAreaCirclePosition.ToVector2D());
                Canvas.SetLeft(control, circleCanvasPosition.X - control.Width / 2);
                Canvas.SetTop(control, circleCanvasPosition.Y - control.Height / 2);
                Panel.SetZIndex(control, 1);
                this.worldMapController.AddControl(control, scaleWithZoom: false);
                this.visualizedSearchAreas.Add((mark, control));
                ToolTipServiceExtend.SetToolTip(control,
                                                string.Format(TooltipDepositSearchAreaFormat,
                                                              mark.ProtoWorldObject.Name));
            }

            this.TryCreateNotification(mark);
        }

        private void MarkDepositClaimCooldownDurationReceivedHandler()
        {
            foreach (var mark in WorldMapResourceMarksSystem.SharedEnumerateMarks())
            {
                this.TryCreateNotification(mark);
            }
        }

        private void MarkRemovedHandler(WorldMapResourceMark mark)
        {
            for (var index = 0; index < this.visualizedMarks.Count; index++)
            {
                var entry = this.visualizedMarks[index];
                if (!entry.mark.Equals(mark))
                {
                    continue;
                }

                this.visualizedMarks.RemoveAt(index);
                this.worldMapController.RemoveControl(entry.mapControl);
                this.RemoveNotification(mark, quick: true);
            }

            for (var index = 0; index < this.visualizedSearchAreas.Count; index++)
            {
                var entry = this.visualizedSearchAreas[index];
                if (!entry.mark.Equals(mark))
                {
                    continue;
                }

                this.visualizedSearchAreas.RemoveAt(index);
                this.worldMapController.RemoveControl(entry.mapControl);
            }
        }

        private void RemoveNotification(in WorldMapResourceMark mark, bool quick)
        {
            for (var index = 0; index < this.notifications.Count; index++)
            {
                var pair = this.notifications[index];
                if (!pair.mark.Equals(mark))
                {
                    continue;
                }

                pair.notification.Hide(quick);
                this.notifications.RemoveAt(index--);
            }
        }

        private void TryCreateNotification(WorldMapResourceMark mark)
        {
            if (!this.enableNotifications)
            {
                return;
            }

            var timeRemains = (int)WorldMapResourceMarksSystem.SharedCalculateTimeToClaimLimitRemovalSeconds(
                mark.ServerSpawnTime);

            if (timeRemains < 20)
            {
                // less than 20 seconds remains - not worth a notification
                return;
            }

            // resource spawned recently
            if (Api.IsEditor)
            {
                return;
            }

            if (!PerkClaimDeposits.Instance
                                  .SharedIsPerkUnlocked(Api.Client.Characters.CurrentPlayerCharacter))
            {
                // don't notify player as perk is not unlocked
                return;
            }

            var notification = this.FindNotification(in mark);
            if (!(notification is null))
            {
                // notification already exist
                return;
            }

            // notify player about the new resource
            notification = NotificationSystem.ClientShowNotification(
                title: Notification_NewResourceAvailable_Title,
                message: GetUpdatedRecentResourceNotificationText(mark,
                                                                  timeRemains),
                icon: mark.ProtoWorldObject.Icon,
                autoHide: false);

            this.AddNotification(mark, notification);
            this.UpdateNotification(mark, notification);
        }

        private void UpdateNotification(WorldMapResourceMark mark, HUDNotificationControl notification)
        {
            if (notification.IsHiding)
            {
                return;
            }

            if (!WorldMapResourceMarksSystem.SharedIsContainsMark(mark))
            {
                this.RemoveNotification(mark, quick: true);
                return;
            }

            var timeRemains = (int)WorldMapResourceMarksSystem
                .SharedCalculateTimeToClaimLimitRemovalSeconds(mark.ServerSpawnTime);
            if (timeRemains <= 0)
            {
                this.RemoveNotification(mark, quick: false);
                return;
            }

            notification.SetMessage(
                GetUpdatedRecentResourceNotificationText(mark,
                                                         timeRemains));

            // schedule recursive update in a second
            ClientTimersSystem.AddAction(
                delaySeconds: 1,
                () => this.UpdateNotification(mark, notification));
        }
    }
}