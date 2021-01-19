namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Teleport
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Teleport.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    public partial class WindowTeleportConfirmationDialog : BaseUserControlWithWindow
    {
        private readonly Vector2Ushort teleportWorldPosition;

        private ViewModelWindowTeleportConfirmationDialog viewModel;

        private WindowTeleportConfirmationDialog(Vector2Ushort teleportWorldPosition)
        {
            this.teleportWorldPosition = teleportWorldPosition;
        }

        public static WindowTeleportConfirmationDialog Instance { get; private set; }

        public static void ShowDialog(Vector2Ushort teleportWorldPosition)
        {
            Api.Client.UI.LayoutRootChildren.Add(
                new WindowTeleportConfirmationDialog(teleportWorldPosition));
        }

        protected override void OnLoaded()
        {
            this.DataContext
                = this.viewModel
                      = new ViewModelWindowTeleportConfirmationDialog(this.teleportWorldPosition);
            Instance = this;
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;

            if (ReferenceEquals(Instance, this))
            {
                Instance = null;
            }
        }
    }
}