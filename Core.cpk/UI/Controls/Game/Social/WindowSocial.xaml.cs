namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Social
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Politics;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Social.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

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
        }

        protected override void WindowOpening()
        {
            base.WindowOpening();
            this.GetByName<OfflineRaidingProtectionControl>("OfflineRaidingProtectionControl")
                .Refresh();
        }
    }
}