namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Controllers
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.CBND.GameApi.ServicesClient.Servers;

    public class MultiplayerMenuServersLanController : BaseMultiplayerMenuServersController
    {
        private static readonly IMasterServerService Provider = Api.Client.MasterServer;

        private bool isDisposed;

        public MultiplayerMenuServersLanController(
            ServerViewModelsProvider serverViewModelsProvider)
            : base(serverViewModelsProvider)
        {
            Provider.LanServerDiscovered += this.LanServerDiscoveredHandler;
            this.IsListAvailable = true;
        }

        public override bool ContainsServerAddress(ServerAddress address)
        {
            return Provider.LanServerAddresses.Contains(address);
        }

        public override void Dispose()
        {
            if (this.isDisposed)
            {
                return;
            }

            this.isDisposed = true;
            Provider.LanServerDiscovered -= this.LanServerDiscoveredHandler;
            base.Dispose();
        }

        public override void ReloadServersList()
        {
            this.RebuildServersList();
        }

        protected override void PopulateServersList()
        {
            foreach (var serverAddress in Provider.LanServerAddresses)
            {
                var viewModelServer = this.ServerViewModelsProvider.GetServerInfoViewModel(serverAddress);
                this.ServerAddressToServerViewModel.Add((serverAddress, viewModelServer));
            }
        }

        private void LanServerDiscoveredHandler(ServerAddress address)
        {
            this.RebuildServersList();
        }
    }
}