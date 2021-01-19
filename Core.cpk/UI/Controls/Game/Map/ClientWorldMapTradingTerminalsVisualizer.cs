namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.Systems.TradingStations;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map.Data;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ClientWorldMapTradingTerminalsVisualizer : BaseWorldMapVisualizer
    {
        private readonly Dictionary<uint, FrameworkElement> markers
            = new();

        private SuperObservableCollection<TradingStationsMapMarksSystem.TradingStationMark> marksProvider;

        public ClientWorldMapTradingTerminalsVisualizer(WorldMapController worldMapController)
            : base(worldMapController)
        {
            this.marksProvider = TradingStationsMapMarksSystem.ClientTradingStationMarksList;
            this.marksProvider.CollectionChanged += this.MarksProviderChangedHandler;
            this.Reset();
        }

        protected override void DisposeInternal()
        {
            this.marksProvider.CollectionChanged -= this.MarksProviderChangedHandler;
            this.marksProvider = null;
            this.Reset();
        }

        private void AddMarker(TradingStationsMapMarksSystem.TradingStationMark mark)
        {
            var tradingStationId = mark.TradingStationId;
            if (this.markers.ContainsKey(tradingStationId))
            {
                Api.Logger.Warning("Trading stations marks already has the map visualizer: " + tradingStationId);
                return;
            }

            var mapControl = new WorldMapMarkTradingTerminal(mark.TradingStationId, mark.IsOwner);
            var canvasPosition = this.WorldToCanvasPosition(mark.TilePosition.ToVector2D());
            Canvas.SetLeft(mapControl, canvasPosition.X);
            Canvas.SetTop(mapControl, canvasPosition.Y);
            Panel.SetZIndex(mapControl, 9);

            this.AddControl(mapControl);

            this.markers[tradingStationId] = mapControl;
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
                    this.RemoveMarker(((TradingStationsMapMarksSystem.TradingStationMark)oldItem).TradingStationId);
                }
            }

            if (e.NewItems is not null)
            {
                foreach (var newItem in e.NewItems)
                {
                    this.AddMarker((TradingStationsMapMarksSystem.TradingStationMark)newItem);
                }
            }
        }

        private void RemoveMarker(uint tradingStationId)
        {
            if (!this.markers.TryGetValue(tradingStationId, out var control))
            {
                return;
            }

            this.markers.Remove(tradingStationId);
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