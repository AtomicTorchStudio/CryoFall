namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map.Data;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ClientWorldMapDroppedItemsVisualizer : BaseWorldMapVisualizer
    {
        private readonly NetworkSyncList<DroppedLootInfo> droppedItemsLocations;

        private readonly Dictionary<DroppedLootInfo, FrameworkElement> markers
            = new();

        public ClientWorldMapDroppedItemsVisualizer(WorldMapController worldMapController)
            : base(worldMapController)
        {
            var playerCharacter = Api.Client.Characters.CurrentPlayerCharacter;
            this.droppedItemsLocations = PlayerCharacter.GetPrivateState(playerCharacter)
                                                        .DroppedLootLocations;

            this.droppedItemsLocations.ClientElementInserted += this.MarkerAddedHandler;
            this.droppedItemsLocations.ClientElementRemoved += this.MarkerRemovedHandler;

            foreach (var mark in this.droppedItemsLocations)
            {
                this.AddMarker(mark);
            }
        }

        protected override void DisposeInternal()
        {
            this.droppedItemsLocations.ClientElementInserted -= this.MarkerAddedHandler;
            this.droppedItemsLocations.ClientElementRemoved -= this.MarkerRemovedHandler;

            if (this.markers.Count > 0)
            {
                foreach (var mark in this.markers.Keys.ToList())
                {
                    this.RemoveMarker(mark);
                }
            }
        }

        private void AddMarker(DroppedLootInfo droppedLootInfo)
        {
            if (this.markers.ContainsKey(droppedLootInfo))
            {
                Api.Logger.Warning("Dropped items already has the map visualizer: " + droppedLootInfo);
                return;
            }

            var mapControl = new WorldMapMarkDroppedItems();
            var canvasPosition = this.WorldToCanvasPosition(droppedLootInfo.Position.ToVector2D());
            Canvas.SetLeft(mapControl, canvasPosition.X);
            Canvas.SetTop(mapControl, canvasPosition.Y);
            Panel.SetZIndex(mapControl, 19);

            this.AddControl(mapControl);

            this.markers[droppedLootInfo] = mapControl;
        }

        private void MarkerAddedHandler(
            NetworkSyncList<DroppedLootInfo> droppedLootInfos,
            int index,
            DroppedLootInfo droppedLootInfo)
        {
            this.AddMarker(droppedLootInfo);
        }

        private void MarkerRemovedHandler(
            NetworkSyncList<DroppedLootInfo> droppedLootInfos,
            int index,
            DroppedLootInfo droppedLootInfo)
        {
            this.RemoveMarker(droppedLootInfo);
        }

        private void RemoveMarker(DroppedLootInfo position)
        {
            if (!this.markers.TryGetValue(position, out var control))
            {
                return;
            }

            this.markers.Remove(position);
            this.RemoveControl(control);
        }
    }
}