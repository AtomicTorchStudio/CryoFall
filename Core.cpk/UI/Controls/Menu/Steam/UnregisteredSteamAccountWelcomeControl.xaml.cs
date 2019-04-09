namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Steam
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Steam.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class UnregisteredSteamAccountWelcomeControl : BaseUserControl
    {
        private ViewModelSteamAccountLinkingWelcome viewModel;

        protected override void OnLoaded()
        {
            this.DataContext = this.viewModel = new ViewModelSteamAccountLinkingWelcome();
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }
    }
}