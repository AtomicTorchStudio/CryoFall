namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map.Data;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ClientWorldMapDroppedItemsVisualizer : IDisposable, IWorldMapVisualizer
    {
        private readonly NetworkSyncList<Vector2Ushort> droppedItemsLocations;

        private readonly Dictionary<Vector2Ushort, FrameworkElement> markers
            = new Dictionary<Vector2Ushort, FrameworkElement>();

        private readonly WorldMapController worldMapController;

        public ClientWorldMapDroppedItemsVisualizer(WorldMapController worldMapController)
        {
            this.worldMapController = worldMapController;

            var playerCharacter = Api.Client.Characters.CurrentPlayerCharacter;
            this.droppedItemsLocations = PlayerCharacter.GetPrivateState(playerCharacter)
                                                        .DroppedItemsLocations;

            this.droppedItemsLocations.ClientElementInserted += this.MarkerAddedHandler;
            this.droppedItemsLocations.ClientElementRemoved += this.MarkerRemovedHandler;

            foreach (var position in this.droppedItemsLocations)
            {
                this.AddMarker(position);
            }
        }

        public bool IsEnabled { get; set; }

        public void Dispose()
        {
            this.droppedItemsLocations.ClientElementInserted -= this.MarkerAddedHandler;
            this.droppedItemsLocations.ClientElementRemoved -= this.MarkerRemovedHandler;

            if (this.markers.Count > 0)
            {
                foreach (var position in this.markers.Keys.ToList())
                {
                    this.RemoveMarker(position);
                }
            }
        }

        private void AddMarker(Vector2Ushort position)
        {
            if (this.markers.ContainsKey(position))
            {
                Api.Logger.Warning("Dropped items already has the map visualizer: " + position);
                return;
            }

            var mapControl = new WorldMapMarkDroppedItems();
            var canvasPosition = this.worldMapController.WorldToCanvasPosition(position.ToVector2D());
            Canvas.SetLeft(mapControl, canvasPosition.X);
            Canvas.SetTop(mapControl, canvasPosition.Y);
            Panel.SetZIndex(mapControl, 12);

            this.worldMapController.AddControl(mapControl);

            this.markers[position] = mapControl;
        }

        private void MarkerAddedHandler(NetworkSyncList<Vector2Ushort> source, int index, Vector2Ushort value)
        {
            this.AddMarker(value);
        }

        private void MarkerRemovedHandler(NetworkSyncList<Vector2Ushort> source, int index, Vector2Ushort removedValue)
        {
            this.RemoveMarker(removedValue);
        }

        private void RemoveMarker(Vector2Ushort position)
        {
            if (!this.markers.TryGetValue(position, out var control))
            {
                return;
            }

            this.markers.Remove(position);
            this.worldMapController.RemoveControl(control);
        }
    }
}