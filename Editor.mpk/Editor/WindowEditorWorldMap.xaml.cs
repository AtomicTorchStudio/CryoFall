namespace AtomicTorch.CBND.CoreMod.Editor
{
    using System;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Editor.Scripts;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Primitives;

    public partial class WindowEditorWorldMap : BaseWindowMenu
    {
        private ControlWorldMap controlWorldMap;

        private BaseWorldMapVisualizer[] visualizers = Array.Empty<BaseWorldMapVisualizer>();

        protected override void InitMenu()
        {
            base.InitMenu();
            this.controlWorldMap = this.GetByName<ControlWorldMap>("ControlWorldMap");
        }

        protected override void OnLoaded()
        {
            this.controlWorldMap.Loaded += this.ControlWorldMapLoadedHandler;

            if (this.controlWorldMap.IsLoaded)
            {
                this.ControlWorldMapLoadedHandler(null, null);
            }
        }

        protected override void OnUnloaded()
        {
            this.controlWorldMap.Loaded -= this.ControlWorldMapLoadedHandler;
            this.DestroyVisualizers();
        }

        protected override void WindowClosed()
        {
            this.DestroyVisualizers();
            base.WindowClosed();
        }

        protected override void WindowOpening()
        {
            this.TryCreateVisualizers();
            this.TryActivateWorldMapController();
            base.WindowOpening();
        }

        private static void MapClickHandler(Vector2D worldPosition)
        {
            EditorSystem.ClientTeleport(worldPosition);
        }

        private void ControlWorldMapLoadedHandler(object sender, RoutedEventArgs e)
        {
            this.TryCreateVisualizers();
        }

        private void DestroyVisualizers()
        {
            if (this.controlWorldMap.WorldMapController is not null)
            {
                this.controlWorldMap.WorldMapController.IsActive = false;
            }

            if (this.visualizers.Length == 0)
            {
                return;
            }

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
        }

        private void TryActivateWorldMapController()
        {
            if (this.controlWorldMap.WorldMapController is null)
            {
                return;
            }

            switch (this.Window.State)
            {
                case GameWindowState.Opening:
                case GameWindowState.Opened:
                    this.controlWorldMap.WorldMapController.IsActive = true;
                    this.controlWorldMap.WorldMapController.MapClickCallback = MapClickHandler;

                    foreach (var visualizer in this.visualizers)
                    {
                        visualizer.IsEnabled = true;
                    }

                    break;
            }
        }

        private void TryCreateVisualizers()
        {
            if (!this.controlWorldMap.IsLoaded)
            {
                return;
            }

            if (this.visualizers.Length > 0)
            {
                return;
            }

            var controller = this.controlWorldMap.WorldMapController;
            if (controller is null)
            {
                return;
            }

            this.visualizers = new BaseWorldMapVisualizer[]
            {
                new ClientWorldMapResourcesVisualizer(controller, enableNotifications: true),
                new ClientWorldMapEventVisualizer(controller)
            };

            this.TryActivateWorldMapController();
            controller.CenterMapOnPlayerCharacter(true);
        }
    }
}