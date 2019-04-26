namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.Perks;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Deposits;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.WorldMapResourceMarks;
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
              [br]{3} minutes.
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

            foreach (var mark in WorldMapResourceMarksSystem.ClientEnumerateMarks())
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

        private static string GetUpdatedRecentResourceNotificationText(
            IProtoStaticWorldObject protoResource,
            Vector2Ushort tilePosition,
            int timeToClaimLimitRemovalMinutes)
        {
            if (timeToClaimLimitRemovalMinutes < 1)
            {
                timeToClaimLimitRemovalMinutes = 1;
            }

            return string.Format(Notification_NewResourceAvailable_MessageFormat,
                                 protoResource.Name,
                                 tilePosition.X,
                                 tilePosition.Y,
                                 timeToClaimLimitRemovalMinutes);
        }

        private FrameworkElement GetMapControl(WorldMapResourceMark mark)
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

        private void MarkAddedHandler(WorldMapResourceMark mark)
        {
            var mapControl = this.GetMapControl(mark);
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

            var timeToClaimLimitRemovalMinutes = WorldMapResourceMarksSystem.SharedCalculateTimeToClaimLimitRemovalMinutes(mark.ServerSpawnTime);
            if (timeToClaimLimitRemovalMinutes < 1)
            {
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

            NotificationSystem.ClientShowNotification(
                title: Notification_NewResourceAvailable_Title,
                message: GetUpdatedRecentResourceNotificationText(protoResource,
                                                                  tilePosition,
                                                                  timeToClaimLimitRemovalMinutes),
                icon: protoResource.Icon,
                autoHide: false);
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