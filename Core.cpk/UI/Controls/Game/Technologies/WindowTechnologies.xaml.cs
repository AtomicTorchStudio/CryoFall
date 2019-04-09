namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Technologies
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Technologies.Data;

    public partial class WindowTechnologies : BaseWindowMenu
    {
        private ViewModelWindowTechnologies viewModel;

        protected override void DisposeMenu()
        {
            base.DisposeMenu();
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }

        protected override void InitMenu()
        {
            this.DataContext = this.viewModel = new ViewModelWindowTechnologies();
        }

        protected override void WindowClosed()
        {
            base.WindowClosed();
            this.viewModel.ListSelectedTechGroup = null;
        }
    }
}