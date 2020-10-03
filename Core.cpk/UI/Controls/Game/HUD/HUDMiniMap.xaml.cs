namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public partial class HUDMiniMap : BaseUserControl
    {
        public static readonly DependencyProperty ViewModelControlWorldMapProperty =
            DependencyProperty.Register(nameof(ViewModelControlWorldMap),
                                        typeof(ViewModelControlWorldMap),
                                        typeof(HUDMiniMap),
                                        new PropertyMetadata(default(ViewModelControlWorldMap)));

        private PanningPanel panningPanel;

        private ViewModelHUDMiniMap viewModel;

        private IWorldMapVisualizer[] visualisers;

        private WorldMapControllerMiniMap worldMapController;

        public ViewModelControlWorldMap ViewModelControlWorldMap
        {
            get => (ViewModelControlWorldMap)this.GetValue(ViewModelControlWorldMapProperty);
            set => this.SetValue(ViewModelControlWorldMapProperty, value);
        }

        protected override void InitControl()
        {
            this.panningPanel = this.GetByName<PanningPanel>("PanningPanel");
            var controlTemplatePlayerMark = this.GetResource<ControlTemplate>("PlayerMarkControlTemplate");
            
            var viewModelControlWorldMap = new ViewModelControlWorldMap();
            this.worldMapController = new WorldMapControllerMiniMap(
                this.panningPanel,
                viewModelControlWorldMap,
                isPlayerMarkDisplayed: true,
                isCurrentCameraViewDisplayed: true,
                isListeningToInput: false,
                paddingChunks: 1,
                // map area size will be changed later anyway
                mapAreaSize: (100, 100),
                controlTemplatePlayerMark);
            this.ViewModelControlWorldMap = viewModelControlWorldMap;
        }

        protected override void OnLoaded()
        {
            var controller = this.worldMapController;

            var landClaimGroupVisualizer = new ClientWorldMapLandClaimsGroupVisualizer(controller);
            this.visualisers = new IWorldMapVisualizer[]
            {
                landClaimGroupVisualizer,
                new ClientWorldMapLandClaimVisualizer(controller, landClaimGroupVisualizer),
                new ClientWorldMapBedVisualizer(controller),
                new ClientWorldMapDroppedItemsVisualizer(controller),
                new ClientWorldMapTradingTerminalsVisualizer(controller),
                new ClientWorldMapResourcesVisualizer(controller, enableNotifications: false),
                new ClientWorldMapEventVisualizer(controller, enableNotifications: false),
                new ClientWorldMapPartyMembersVisualizer(controller),
                new ClientWorldMapLastVehicleVisualizer(controller)
            };

            foreach (var visualiser in this.visualisers)
            {
                visualiser.IsEnabled = true;
            }

            this.viewModel = new ViewModelHUDMiniMap(this,
                                                     callbackSizeOrZoomChanged: this.RefreshMap);
            this.DataContext = this.viewModel;

            this.RefreshMap();

            controller.CenterMapOnPlayerCharacter(resetZoomIfBelowThreshold: false);

            this.MouseWheel += this.MouseWheelHandler;
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel?.Dispose();
            this.viewModel = null;

            this.worldMapController.IsActive = false;

            foreach (var visualiser in this.visualisers)
            {
                try
                {
                    visualiser.Dispose();
                }
                catch (Exception ex)
                {
                    Api.Logger.Exception(ex, "Exception during visualizer disposing");
                }
            }

            this.visualisers = Array.Empty<IWorldMapVisualizer>();

            this.MouseWheel -= this.MouseWheelHandler;
        }

        private void MouseWheelHandler(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
            var wheelRotation = e.Delta * 0.5 * PanningPanel.MouseScrollWheelZoomSpeed;
            // use exponential scale https://www.gamedev.net/forums/topic/666225-equation-for-zooming/?tab=comments#comment-5213633
            var zoom = Math.Exp(Math.Log(this.viewModel.Zoom) + wheelRotation);
            zoom = MathHelper.Clamp(zoom, this.viewModel.ZoomMin, this.viewModel.ZoomMax);
            this.viewModel.Zoom = zoom;
        }

        private void RefreshMap()
        {
            if (this.viewModel is null)
            {
                return;
            }

            var zoom = this.viewModel.Zoom;
            this.panningPanel.SetZoom(zoom);

            var scale = 1 / (4.0 * zoom);
            var mapAreaSize = new Vector2Ushort(
                (ushort)(this.viewModel.ControlWidth * scale + ScriptingConstants.WorldChunkSize),
                (ushort)(this.viewModel.ControlHeight * scale + ScriptingConstants.WorldChunkSize));
            this.worldMapController.MapAreaSize = mapAreaSize;

            //Api.Logger.Dev($"Map zoom: {zoom:F2} scale: {scale:F3} map size: {mapAreaSize}");

            var isActive = this.viewModel.IsMapVisible;
            this.worldMapController.IsActive = isActive;

            foreach (var visualiser in this.visualisers)
            {
                visualiser.IsEnabled = isActive;
            }
        }
    }
}