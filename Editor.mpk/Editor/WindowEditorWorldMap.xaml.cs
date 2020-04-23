namespace AtomicTorch.CBND.CoreMod.Editor
{
    using System;
    using AtomicTorch.CBND.CoreMod.Editor.Scripts;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Primitives;

    public partial class WindowEditorWorldMap : BaseWindowMenu
    {
        private ControlWorldMap controlWorldMap;

        private IWorldMapVisualizer[] visualisers;

        protected override void InitMenu()
        {
            base.InitMenu();
            this.controlWorldMap = this.GetByName<ControlWorldMap>("ControlWorldMap");
        }

        protected override void WindowClosed()
        {
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

            this.controlWorldMap.WorldMapController.IsActive = false;

            base.WindowClosed();
        }

        protected override void WindowOpened()
        {
            base.WindowOpened();
            this.controlWorldMap.WorldMapController.MapClickCallback = MapClickHandler;
        }

        protected override void WindowOpening()
        {
            var controller = this.controlWorldMap.WorldMapController;

            this.visualisers = new IWorldMapVisualizer[]
            {
                new ClientWorldMapResourcesVisualizer(controller, enableNotifications: true),
                new ClientWorldMapEventVisualizer(controller, enableNotifications: true)
            };

            controller.IsActive = true;

            base.WindowOpening();
        }

        private static void MapClickHandler(Vector2D worldPosition)
        {
            EditorSystem.ClientTeleport(worldPosition);
        }
    }
}