namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Perks;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Deposits;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.WorldMapResourceMarks;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Notifications;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map.Data;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

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

        private readonly List<(WorldMapResourceMark mark, HUDNotificationControl notification)> notifications
            = new List<(WorldMapResourceMark mark, HUDNotificationControl notification)>();

        private readonly List<(WorldMapResourceMark mark, FrameworkElement mapControl)> visualizedMarks
            = new List<(WorldMapResourceMark, FrameworkElement)>();

        private readonly WorldMapController worldMapController;

        public ClientWorldMapResourcesVisualizer(WorldMapController worldMapController)
        {
            this.worldMapController = worldMapController;

            WorldMapResourceMarksSystem.ClientMarkAdded += this.MarkAddedHandler;
            WorldMapResourceMarksSystem.ClientMarkRemoved += this.MarkRemovedHandler;

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
                    return new WorldMapMarkResourceOil();

                case ObjectDepositGeothermalSpring _:
                    return new WorldMapMarkResourceLithium();

                default:
                    return null;
            }
        }

        private static string GetUpdatedRecentResourceNotificationText(
            IProtoStaticWorldObject protoResource,
            Vector2Ushort tilePosition,
            int timeRemains)
        {
            if (timeRemains < 1)
            {
                timeRemains = 1;
            }

            var localPosition = tilePosition - Api.Client.World.WorldBounds.Offset;
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

        private void MarkAddedHandler(WorldMapResourceMark mark)
        {
            var mapControl = GetMapControl(mark);
            var protoResource = mark.ProtoWorldObject;

            if (mapControl == null)
            {
                Api.Logger.Warning("Unknown world object mark: "
                                   + protoResource
                                   + " - there is no UI control for this world object prototype");
                return;
            }

            var canvasPosition = this.worldMapController.WorldToCanvasPosition(mark.Position.ToVector2D());
            Canvas.SetLeft(mapControl, canvasPosition.X);
            Canvas.SetTop(mapControl, canvasPosition.Y);
            Panel.SetZIndex(mapControl, 16);

            this.worldMapController.AddControl(mapControl);

            this.visualizedMarks.Add((mark, mapControl));

            var timeRemains = (int)WorldMapResourceMarksSystem.SharedCalculateTimeToClaimLimitRemovalSeconds(
                mark.ServerSpawnTime);
            if (timeRemains < 60)
            {
                // less than a minute - not worth a notification
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

            // notify player about the new resource
            var notification = NotificationSystem.ClientShowNotification(
                title: Notification_NewResourceAvailable_Title,
                message: GetUpdatedRecentResourceNotificationText(protoResource,
                                                                  mark.Position,
                                                                  timeRemains),
                icon: protoResource.Icon,
                autoHide: false);

            this.AddNotification(mark, notification);
            this.UpdateNotification(mark, notification);
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

            var protoResource = mark.ProtoWorldObject;
            var timeRemains = (int)WorldMapResourceMarksSystem
                .SharedCalculateTimeToClaimLimitRemovalSeconds(mark.ServerSpawnTime);
            if (timeRemains <= 0)
            {
                this.RemoveNotification(mark, quick: false);
                return;
            }

            notification.SetMessage(
                GetUpdatedRecentResourceNotificationText(protoResource,
                                                         mark.Position,
                                                         timeRemains));

            // schedule recursive update in a second
            ClientTimersSystem.AddAction(
                delaySeconds: 1,
                () => this.UpdateNotification(mark, notification));
        }
    }
}