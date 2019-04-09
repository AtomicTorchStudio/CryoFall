namespace AtomicTorch.CBND.CoreMod.Editor
{
    using AtomicTorch.CBND.CoreMod.Editor.Scripts;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Primitives;

    public partial class WindowEditorWorldMap : BaseWindowMenu
    {
        private ControlWorldMap controlWorldMap;

        private ClientWorldMapResourcesVisualizer resourcesVisualizer;

        protected override void InitMenu()
        {
            base.InitMenu();
            this.controlWorldMap = this.GetByName<ControlWorldMap>("ControlWorldMap");
        }

        protected override void WindowClosed()
        {
            this.resourcesVisualizer.Dispose();
            this.resourcesVisualizer = null;

            this.controlWorldMap.WorldMapController.IsActive = false;

            base.WindowClosed();
        }

        protected override void WindowOpened()
        {
            base.WindowOpened();
            this.controlWorldMap.WorldMapController.MapClickCallback = this.MapClickHandler;
        }

        protected override void WindowOpening()
        {
            var controller = this.controlWorldMap.WorldMapController;

            this.resourcesVisualizer = new ClientWorldMapResourcesVisualizer(controller);

            controller.IsActive = true;
            this.controlWorldMap.CenterMapOnPlayerCharacter();

            base.WindowOpening();
        }

        private void MapClickHandler(Vector2D worldPosition)
        {
            EditorSystem.ClientTeleport(worldPosition);
        }
    }
}