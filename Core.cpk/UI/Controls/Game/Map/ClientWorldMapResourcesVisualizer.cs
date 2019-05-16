namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Timer;
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

            return string.Format(Notification_NewResourceAvailable_MessageFormat,
                                 protoResource.Name,
                                 tilePosition.X,
                                 tilePosition.Y,
                                 ClientTimeFormatHelper.FormatTimeDuration(timeRemains));
        }

        private static void UpdateNotification(WorldMapResourceMark mark, HUDNotificationControl notification)
        {
            if (notification.IsHiding)
            {
                return;
            }

            var protoResource = mark.ProtoWorldObject;
            var tilePosition = mark.Position;
            var timeRemains =
                (int)WorldMapResourceMarksSystem.SharedCalculateTimeToClaimLimitRemovalSeconds(mark.ServerSpawnTime);
            if (timeRemains <= 0)
            {
                notification.Hide(quick: false);
                return;
            }

            notification.ViewModel.Message = GetUpdatedRecentResourceNotificationText(protoResource,
                                                                                      tilePosition,
                                                                                      timeRemains);

            // schedule recursive update in a second
            ClientComponentTimersManager.AddAction(
                delaySeconds: 1,
                () => UpdateNotification(mark, notification));
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

            var worldPosition = mark.Position.ToVector2D()
                                + protoResource.Layout.Center;
            var canvasPosition = this.worldMapController.WorldToCanvasPosition(worldPosition);
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
            var tilePosition = worldPosition.ToVector2Ushort()
                               - Api.Client.World.WorldBounds.Offset;

            var notification = NotificationSystem.ClientShowNotification(
                title: Notification_NewResourceAvailable_Title,
                message: GetUpdatedRecentResourceNotificationText(protoResource,
                                                                  tilePosition,
                                                                  timeRemains),
                icon: protoResource.Icon,
                autoHide: false);

            UpdateNotification(mark, notification);
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
            }
        }
    }
}