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
            this.SortType = ServersListSortType.OnlinePlayersCount;
            this.provider = Api.Client.MasterServer.ServersProvider.Public;
            this.provider.Updated += this.PublicListUpdatedHandler;
            this.specialCondition = specialCondition;
        }

        public override void Dispose()
        {
            if (this.isDisposed)
            {
                return;
            }

            this.isDisposed = true;
            base.Dispose();
            this.provider.Updated -= this.PublicListUpdatedHandler;
        }

        public override void ReloadServersList()
        {
            this.provider.Clear();
            this.RebuildServersList();
            this.IsListAvailable = false;
            //this.provider.RequestPublicServersList();
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
                var viewModelServerInfo = viewModelServer.ViewModelServerInfo;

                viewModelServerInfo.IsFeatured = publicServerInfo.IsFeatured;
                viewModelServerInfo.IsOfficial = publicServerInfo.IsOfficial;

                if (!viewModelServerInfo.IsInfoReceived)
                {
                    // set info from public info - but never overwrite the data already set from the actual server info
                    viewModelServerInfo.Title = publicServerInfo.Title;
                    viewModelServerInfo.IsModded = !publicServerInfo.IsOfficial
                                                   && publicServerInfo.IsModded;
                    viewModelServerInfo.Version = publicServerInfo.Version;
                    viewModelServerInfo.NetworkProtocolVersion = 0;
                    //viewModelServer.PingMeasurementDone += this.ViewModelServerPingMeasurementDoneHandler;
                    //viewModelServer.TitleSet += this.ViewModelServerTitleSetHandler;
                    //viewModelServer.OnlinePlayersCountSet += this.ViewModelServerOnlinePlayersCountSetHandler;
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