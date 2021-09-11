namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Controllers
{
    using System;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Servers;

    public class MultiplayerMenuServersPublicController : BaseMultiplayerMenuServersController
    {
        private readonly IPublicServersListProvider provider;

        private readonly Func<PublicServerInfo, bool> specialCondition;

        private bool isDisposed;

        public MultiplayerMenuServersPublicController(
            ServerViewModelsProvider serverViewModelsProvider,
            Func<PublicServerInfo, bool> specialCondition)
            : base(serverViewModelsProvider)
        {
            this.provider = Api.Client.MasterServer.ServersProvider.Public;
            this.provider.Updated += this.PublicListUpdatedHandler;
            this.specialCondition = specialCondition;
        }

        public override bool ContainsServerAddress(ServerAddress address)
        {
            foreach (var publicServerInfo in this.provider.List)
            {
                if (!this.specialCondition(publicServerInfo))
                {
                    continue;
                }

                if (publicServerInfo.Address == address)
                {
                    return true;
                }
            }

            return false;
        }

        public override void Dispose()
        {
            if (this.isDisposed)
            {
                return;
            }

            this.isDisposed = true;
            this.provider.Updated -= this.PublicListUpdatedHandler;
            base.Dispose();
        }

        public override void ReloadServersList()
        {
            this.provider.Clear();
            this.RebuildServersList();
            this.IsListAvailable = false;
        }

        protected override void PopulateServersList()
        {
            foreach (var publicServerInfo in this.provider.List)
            {
                if (!this.specialCondition(publicServerInfo))
                {
                    continue;
                }

                var serverAddress = publicServerInfo.Address;
                var viewModelServer = this.ServerViewModelsProvider.GetServerInfoViewModel(serverAddress);
                var item = viewModelServer.ViewModelServerInfo;

                item.IsFeatured = publicServerInfo.IsFeatured;
                item.IsOfficial = publicServerInfo.IsOfficial;
                item.IsCommunity = !publicServerInfo.IsOfficial;

                if (!item.IsInfoReceived
                    && !item.Address.IsLocalServer)
                {
                    // set info from public info - but never overwrite the data already set from the actual server info
                    item.Title = publicServerInfo.Title;
                    item.IsModded = !publicServerInfo.IsOfficial
                                    && publicServerInfo.IsModded;
                    item.Version = publicServerInfo.Version;
                    item.NetworkProtocolVersion = 0;
                }

                this.ServerAddressToServerViewModel.Add((serverAddress, viewModelServer));
            }
        }

        private void PublicListUpdatedHandler(bool isListAvailable)
        {
            this.RebuildServersList();
            this.IsListAvailable = isListAvailable;
        }
    }
}