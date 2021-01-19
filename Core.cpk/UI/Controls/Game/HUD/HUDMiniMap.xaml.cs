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

        private BaseWorldMapVisualizer[] visualizers;

        private WorldMapControllerMiniMap worldMapController;

        public ViewModelControlWorldMap ViewModelControlWorldMap
        {
            get => (ViewModelControlWorldMap)this.GetValue(ViewModelControlWorldMapProperty);
            set => this.SetValue(ViewModelControlWorldMapProperty, value);
        }

        protected override void InitControl()
        {
            this.panningPanel = this.GetByName<PanningPanel>("PanningPanel");
            this.ViewModelControlWorldMap = new ViewModelControlWorldMap();
        }

        protected override void OnLoaded()
        {
            var controller = new WorldMapControllerMiniMap(
                this.panningPanel,
                this.ViewModelControlWorldMap,
                isPlayerMarkDisplayed: true,
                isCurrentCameraViewDisplayed: true,
                isListeningToInput: false,
                paddingChunks: 10,
                // map area size will be changed later anyway
                mapAreaSize: (100, 100),
                sectorProvider: WorldMapSectorProviderHelper.GetProvider(isEditor: false),
                customControlTemplatePlayerMark: this.GetResource<ControlTemplate>("PlayerMarkControlTemplate"));

            this.worldMapController = controller;
            var landClaimGroupVisualizer = new ClientWorldMapLandClaimsGroupVisualizer(controller);
            this.visualizers = new BaseWorldMapVisualizer[]
            {
                landClaimGroupVisualizer,
                new ClientWorldMapLandClaimVisualizer(controller, landClaimGroupVisualizer),
                new ClientWorldMapBedVisualizer(controller),
                new ClientWorldMapDroppedItemsVisualizer(controller),
                new ClientWorldMapTradingTerminalsVisualizer(controller),
                new ClientWorldMapResourcesVisualizer(controller, enableNotifications: false),
                new ClientWorldMapEventVisualizer(controller),
                new ClientWorldMapMembersVisualizer(controller),
                new ClientWorldMapLastVehicleVisualizer(controller),
                new ClientWorldMapTeleportsVisualizer(controller, isActiveMode: false)
            };

            foreach (var visualizer in this.visualizers)
            {
                visualizer.IsEnabled = true;
            }

            this.viewModel = new ViewModelHUDMiniMap(this,
                                                     callbackSizeOrZoomChanged: this.RefreshMap);
            this.DataContext = this.viewModel;

            this.RefreshMap();

            controller.CenterMapOnPlayerCharacter(resetZoomIfBelowThreshold: false);

            this.MouseWheel += this.MouseWheelHandler;
            ClientUpdateHelper.UpdateCallback += this.Update;
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel?.Dispose();
            this.viewModel = null;

            this.worldMapController.Dispose();
            this.worldMapController = null;

            foreach (var visualizer in this.visualizers)
            {
                try
                {
                    visualizer.Dispose();
                }
                catch (Exception ex)
                {
                    Api.Logger.Exception(ex, "Exception during visualizer disposing");
                }
            }

            this.visualizers = Array.Empty<BaseWorldMapVisualizer>();

            this.MouseWheel -= this.MouseWheelHandler;
            ClientUpdateHelper.UpdateCallback -= this.Update;
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

            foreach (var visualizer in this.visualizers)
            {
                visualizer.IsEnabled = isActive;
            }

            this.worldMapController.CenterMapOnPlayerCharacter(resetZoomIfBelowThreshold: false);
        }

        private void Update()
        {
            if (!this.viewModel.IsMouseOverIncludingHidden)
            {
                return;
            }

            var delta = -100 * Api.Client.Input.MouseScrollDeltaValue;
            if (delta == 0)
            {
                return;
            }

            Api.Client.Input.ConsumeMouseScrollDeltaValue();
            var wheelRotation = delta * 0.5 * PanningPanel.MouseScrollWheelZoomSpeed;
            // use exponential scale https://www.gamedev.net/forums/topic/666225-equation-for-zooming/?tab=comments#comment-5213633
            var zoom = Math.Exp(Math.Log(this.viewModel.Zoom) + wheelRotation);
            zoom = MathHelper.Clamp(zoom, this.viewModel.ZoomMin, this.viewModel.ZoomMax);
            this.viewModel.Zoom = zoom;
        }
    }
}