namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Social
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Social.Data;

    public partial class WindowSocial : BaseWindowMenu
    {
        private ViewModelWindowSocial viewModel;

        protected override void DisposeMenu()
        {
            base.DisposeMenu();
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }

        protected override void InitMenu()
        {
            this.DataContext = this.viewModel = new ViewModelWindowSocial();
            this.viewModel.IsActive = true;
        }
    }
}