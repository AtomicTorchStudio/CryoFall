namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Player
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;

    public partial class WindowHandCrafting : BaseWindowMenu
    {
        private ViewModelWindowHandCrafting viewModel;

        protected override void DisposeMenu()
        {
            base.DisposeMenu();
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }

        protected override void InitMenu()
        {
            this.DataContext = this.viewModel = new ViewModelWindowHandCrafting();
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