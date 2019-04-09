namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Player
{
    using System.Windows.Shapes;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class WindowInventory : BaseWindowMenu
    {
        private ViewModelWindowInventory viewModel;

        protected override void DisposeMenu()
        {
            base.DisposeMenu();
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }

        protected override void InitMenu()
        {
            var controlSkeletonView = this.GetByName<Rectangle>("SkeletonViewControl");
            this.DataContext = this.viewModel = new ViewModelWindowInventory(controlSkeletonView);
        }

        protected override void WindowClosed()
        {
            base.WindowClosed();
            this.viewModel.IsActive = false;
        }

        protected override void WindowOpening()
        {
            base.WindowOpening();
            this.viewModel.IsActive = true;
        }
    }
}