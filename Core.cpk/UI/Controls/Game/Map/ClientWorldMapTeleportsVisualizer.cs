namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Misc;
    using AtomicTorch.CBND.CoreMod.Systems.TeleportsSystem;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ClientWorldMapTeleportsVisualizer : BaseWorldMapVisualizer
    {
        private readonly Vector2Ushort? currentTeleportLocation;

        private readonly bool isActiveMode;

        private readonly Dictionary<Vector2Ushort, FrameworkElement> markers
            = new();

        private SuperObservableCollection<Vector2Ushort> marksProvider;

        public ClientWorldMapTeleportsVisualizer(
            WorldMapController worldMapController,
            bool isActiveMode,
            Vector2Ushort? currentTeleportLocation = null)
            : base(worldMapController)
        {
            this.isActiveMode = isActiveMode;
            this.currentTeleportLocation = currentTeleportLocation;

            this.marksProvider = TeleportsSystem.ClientDiscoveredTeleports;
            this.marksProvider.CollectionChanged += this.MarksProviderChangedHandler;
            this.Reset();
        }

        protected override void DisposeInternal()
        {
            this.marksProvider.CollectionChanged -= this.MarksProviderChangedHandler;
            this.marksProvider = null;
            this.Reset();
        }

        private void AddMarker(Vector2Ushort teleportPosition)
        {
            if (this.markers.ContainsKey(teleportPosition))
            {
                Api.Logger.Warning("Teleport already has the map visualizer: " + teleportPosition);
                return;
            }

            FrameworkElement mapControl;

            var teleportTitle = Api.GetProtoEntity<ObjectAlienTeleport>().Name;
            if (!this.isActiveMode)
            {
                mapControl = new WorldMapMarkTeleportInactive()
                {
                    TeleportTitle = teleportTitle
                };

                Panel.SetZIndex(mapControl, 8);
            }
            else
            {
                var isCurrentTeleport = teleportPosition == this.currentTeleportLocation;
                mapControl = new WorldMapMarkTeleportActive()
                {
                    TeleportTitle = teleportTitle,
                    WorldPosition = teleportPosition,
                    IsCurrentTeleport = isCurrentTeleport
                };

                Panel.SetZIndex(mapControl, 20);
            }

            var canvasPosition = this.WorldToCanvasPosition(teleportPosition.ToVector2D() + (2, -1));
            Canvas.SetLeft(mapControl, canvasPosition.X);
            Canvas.SetTop(mapControl, canvasPosition.Y);

            this.AddControl(mapControl, scaleWithZoom: true);

            this.markers[teleportPosition] = mapControl;
        }

        private void MarksProviderChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Move)
            {
                return;
            }

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                this.Reset();
                return;
            }

            if (e.OldItems is not null)
            {
                foreach (var oldItem in e.OldItems)
                {
                    this.RemoveMarker((Vector2Ushort)oldItem);
                }
            }

            if (e.NewItems is not null)
            {
                foreach (var newItem in e.NewItems)
                {
                    this.AddMarker((Vector2Ushort)newItem);
                }
            }
        }

        private void RemoveMarker(Vector2Ushort teleportPosition)
        {
            if (!this.markers.TryGetValue(teleportPosition, out var control))
            {
                return;
            }

            this.markers.Remove(teleportPosition);
            this.RemoveControl(control);
        }

        private void Reset()
        {
            if (this.markers.Count != 0)
            {
                foreach (var mark in this.markers.Keys.ToList())
                {
                    this.RemoveMarker(mark);
                }
            }

            if (this.marksProvider is not null)
            {
                foreach (var mark in this.marksProvider)
                {
                    this.AddMarker(mark);
                }
            }
        }
    }
}