namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Account
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Account.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class UnregisteredExternalAccountWelcomeControl : BaseUserControl
    {
        private ViewModelExternalAccountLinkingWelcome viewModel;

        protected override void OnLoaded()
        {
            this.DataContext = this.viewModel = new ViewModelExternalAccountLinkingWelcome();
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }
    }
}