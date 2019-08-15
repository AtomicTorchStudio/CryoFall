namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Politics
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Politics.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class WindowPolitics : BaseWindowMenu
    {
        private ViewModelWindowPolitics viewModel;

        protected override void DisposeMenu()
        {
            base.DisposeMenu();
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }

        protected override void InitMenu()
        {
            this.DataContext = this.viewModel = new ViewModelWindowPolitics();
        }

        protected override void WindowOpening()
        {
            base.WindowOpening();
            this.GetByName<OfflineRaidingProtectionControl>("OfflineRaidingProtectionControl")
                .Refresh();
        }
    }
}