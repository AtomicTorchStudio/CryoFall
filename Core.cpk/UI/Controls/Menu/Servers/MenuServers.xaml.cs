namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class MenuServers : BaseUserControl
    {
        private ViewModelMenuServers viewModel;

        protected override void InitControl()
        {
        }

        protected override void OnLoaded()
        {
            if (IsDesignTime)
            {
                return;
            }

            this.DataContext = this.viewModel = new ViewModelMenuServers();
        }

        protected override void OnUnloaded()
        {
            if (IsDesignTime)
            {
                return;
            }

            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }
    }
}