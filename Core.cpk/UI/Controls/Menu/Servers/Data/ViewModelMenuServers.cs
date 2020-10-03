namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data
{
    using System;
    using System.Linq;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Controllers;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.CBND.GameApi.ServicesClient.Servers;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ViewModelMenuServers : BaseViewModel
    {
        public const string DialogClearServerList_Message = "Are you sure you want to clear this servers list?";

        public const string DialogClearServerList_Title = "Clear server list";

        private readonly IServersProvider serversProvider = Api.Client.MasterServer.ServersProvider;

        private ViewModelServersList[] allServersLists;

        private ViewModelServerInfoListEntry selectedServer;

        private ServerViewModelsProvider serverViewModelsProvider;

        public ViewModelMenuServers()
        {
#if !GAME
            if (IsDesignTime)
            {
                // design-time data
                this.allServersLists = new ViewModelServersList[0];
                this.SelectedServer = new ViewModelServerInfoListEntry();
                return;
            }
#endif

            Instance?.Dispose();
            Instance = this;

            this.serverViewModelsProvider = ServerViewModelsProvider.Instance;

            this.FeaturedServers =
                new ViewModelServersList(
                    new MultiplayerMenuServersPublicController(
                            this.serverViewModelsProvider,
                            specialCondition: info => info.IsOfficial
                                                      || info.IsFeatured)
                        {
                            DefaultSortType = ServersListSortType.Ping
                        },
                    this.OnSelectedServerChanged);
            this.FeaturedServers.IsActive = true;

            this.CommunityServers =
                new ViewModelServersList(
                    new MultiplayerMenuServersPublicController(
                            this.serverViewModelsProvider,
                            specialCondition: info => !info.IsOfficial
                                                      && !info.IsFeatured
                                                      && !info.IsModded)
                        {
                            DefaultSortType = ServersListSortType.Ping
                        },
                    this.OnSelectedServerChanged);

            this.ModdedServers =
                new ViewModelServersList(
                    new MultiplayerMenuServersPublicController(
                            this.serverViewModelsProvider,
                            specialCondition: info => !info.IsOfficial
                                                      && !info.IsFeatured
                                                      && info.IsModded)
                        {
                            DefaultSortType = ServersListSortType.Ping
                        },
                    this.OnSelectedServerChanged);

            this.CustomServers =
                new ViewModelServersList(
                    new MultiplayerMenuServersController(this.serversProvider.Custom, this.serverViewModelsProvider),
                    this.OnSelectedServerChanged);

            this.FavoriteServers =
                new ViewModelServersList(
                    new MultiplayerMenuServersController(this.serversProvider.Favorite, this.serverViewModelsProvider),
                    this.OnSelectedServerChanged);

            this.HistoryServers =
                new ViewModelServersList(
                    new MultiplayerMenuServersController(this.serversProvider.History, this.serverViewModelsProvider),
                    this.OnSelectedServerChanged);

            this.allServersLists = new[]
            {
                this.FeaturedServers,
                this.CommunityServers,
                this.ModdedServers,
                this.CustomServers,
                this.FavoriteServers,
                this.HistoryServers
            };

            RequestPublicServersList();

            this.serversProvider.ServerPingUpdated += this.ServerPingUpdatedHandled;
            this.serversProvider.ServerInfoReceived += this.ServerInfoReceivedHandler;
            Api.Client.MasterServer.ConnectionStateChanged += MasterServerConnectionStateChangedHandler;
        }

        public static ViewModelMenuServers Instance { get; private set; }

        public BaseCommand CommandAddCustomServer => new ActionCommand(this.ExecuteCommandAddCustomServer);

        public BaseCommand CommandClearFavorites => new ActionCommand(
            () =>
            {
                var viewModels = this.FavoriteServers.ServersList.ToList();
                ExecuteCommandClearHistory(
                    Client.MasterServer.ServersProvider.Favorite,
                    onFinished: () =>
                                {
                                    // let's simply reset the IsFavorite flag
                                    foreach (var viewModel in viewModels)
                                    {
                                        var viewModelServerInfo = viewModel.ViewModelServerInfo;
                                        if (!viewModelServerInfo.IsDisposed)
                                        {
                                            viewModelServerInfo.IsFavorite = false;
                                        }
                                    }
                                });
            });

        public BaseCommand CommandClearHistory => new ActionCommand(
            () => ExecuteCommandClearHistory(
                Client.MasterServer.ServersProvider.History));

        public BaseCommand CommandRefreshAll => new ActionCommand(this.ExecuteCommandRefreshAll);

        public ViewModelServersList CommunityServers { get; }

        public ViewModelServersList CustomServers { get; }

        public ViewModelServersList FavoriteServers { get; }

        public ViewModelServersList FeaturedServers { get; }

        public ViewModelServersList HistoryServers { get; }

        public ViewModelServersList ModdedServers { get; }

        public ViewModelServerInfoListEntry SelectedServer
        {
            get => this.selectedServer;
            set
            {
                if (this.selectedServer == value)
                {
                    return;
                }

                this.selectedServer = value;
                this.NotifyThisPropertyChanged();
                this.SelectedServerVisibility = this.selectedServer is not null ? Visibility.Visible : Visibility.Collapsed;

                //foreach (var viewModelServersList in this.allServersLists)
                //{
                //	viewModelServersList.SelectedServer = this.selectedServer;
                //}
            }
        }

        public Visibility SelectedServerVisibility { get; set; }

        public bool ShowEmptyServers
        {
            get => ServerViewModelsProvider.ShowEmptyServers;
            set
            {
                if (ServerViewModelsProvider.ShowEmptyServers == value)
                {
                    return;
                }

                ServerViewModelsProvider.ShowEmptyServers = value;
                this.NotifyThisPropertyChanged();
            }
        }

        public bool ShowIncompatibleServers
        {
            get => ServerViewModelsProvider.ShowIncompatibleServers;
            set
            {
                if (ServerViewModelsProvider.ShowIncompatibleServers == value)
                {
                    return;
                }

                ServerViewModelsProvider.ShowIncompatibleServers = value;
                this.NotifyThisPropertyChanged();
            }
        }

        public bool ShowPvEServers
        {
            get => ServerViewModelsProvider.ShowPvEServers;
            set
            {
                if (ServerViewModelsProvider.ShowPvEServers == value)
                {
                    return;
                }

                if (!value
                    && !ServerViewModelsProvider.ShowPvPServers)
                {
                    // don't allow disabling the last checkbox
                    return;
                }

                ServerViewModelsProvider.ShowPvEServers = value;
                this.NotifyThisPropertyChanged();
            }
        }

        public bool ShowPvPServers
        {
            get => ServerViewModelsProvider.ShowPvPServers;
            set
            {
                if (ServerViewModelsProvider.ShowPvPServers == value)
                {
                    return;
                }

                if (!value
                    && !ServerViewModelsProvider.ShowPvEServers)
                {
                    // don't allow disabling the last checkbox
                    return;
                }

                ServerViewModelsProvider.ShowPvPServers = value;
                this.NotifyThisPropertyChanged();
            }
        }

        public ViewModelServerInfoListEntry GetSelectedItem()
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var serversList in this.allServersLists)
            {
                if (serversList.IsActive)
                {
                    return serversList.SelectedServer;
                }
            }

            return null;
        }

        public void ResetSortOrder()
        {
            foreach (var list in this.allServersLists)
            {
                list.SortType = list.Controller.DefaultSortType;
                list.IsSortOrderReversed = false;
            }
        }

        protected override void DisposeViewModel()
        {
            try
            {
                Api.Client.MasterServer.ConnectionStateChanged -= MasterServerConnectionStateChangedHandler;
            }
            catch (Exception ex)
            {
                Api.Logger.Exception(ex);
            }

            if (Instance == this)
            {
                Instance = null;
            }

            foreach (var viewModelServersList in this.allServersLists)
            {
                viewModelServersList.Dispose();
            }

            this.allServersLists = null;

            base.DisposeViewModel();

            this.serverViewModelsProvider = null;
        }

        private static void ExecuteCommandClearHistory(
            IServersListProvider list,
            Action onFinished = null)
        {
            DialogWindow.ShowDialog(
                DialogClearServerList_Title,
                DialogClearServerList_Message,
                okAction: () =>
                          {
                              list.Clear();
                              list.Save();
                              onFinished?.Invoke();
                          },
                okText: CoreStrings.Yes,
                hideCancelButton: false);
        }

        private static void MasterServerConnectionStateChangedHandler()
        {
            if (Api.Client.MasterServer.MasterServerConnectionState == ConnectionState.Connected)
            {
                RequestPublicServersList();
            }
        }

        private static void RequestPublicServersList()
        {
            Client.MasterServer.ServersProvider.Public.RequestPublicServersList();
        }

        private void ExecuteCommandAddCustomServer()
        {
            var window = new WindowAddOrEditServer
            {
                WindowTitle = CoreStrings.MenuServers_Button_AddCustomServer,
                ActionTitle = CoreStrings.Button_Save,
                OkAction = newServerName =>
                           {
                               Logger.Important($"Server added: {newServerName}");
                               ServerAddress newServerAddress;
                               newServerAddress = AtomicGuid.TryParse(newServerName, out var guid)
                                                      ? new ServerAddress(guid)
                                                      : new ServerAddress(newServerName);

                               var provider = Client.MasterServer.ServersProvider;
                               provider.Custom.Add(newServerAddress);
                               provider.Custom.Save();

                               // select custom servers list
                               foreach (var serversList in this.allServersLists)
                               {
                                   serversList.IsActive = serversList == this.CustomServers;
                               }

                               this.CustomServers.SelectedServer =
                                   this.CustomServers.ServersList.FirstOrDefault(
                                       s => s.ViewModelServerInfo.Address == newServerAddress);

                               // TODO: ensure the new entry is refreshed first
                           }
            };

            Client.UI.LayoutRootChildren.Add(window);
        }

        private void ExecuteCommandRefreshAll()
        {
            RequestPublicServersList();
            this.serverViewModelsProvider.RefreshAll();
        }

        private void OnSelectedServerChanged(ViewModelServerInfoListEntry viewModelServerInfoListEntry)
        {
            this.SelectedServer = viewModelServerInfoListEntry;
        }

        private void ServerInfoReceivedHandler(ServerInfo serverinfo)
        {
            foreach (var serversList in this.allServersLists)
            {
                if (!serversList.IsActive)
                {
                    continue;
                }

                switch (serversList.Controller.SortType)
                {
                    case ServersListSortType.OnlinePlayersCount:
                    case ServersListSortType.LastWipe:
                        serversList.ScheduleSortEntries();
                        break;
                    // Please note: we don't sort on server info receive if the list is ordered by ping
                }
            }
        }

        private void ServerPingUpdatedHandled(ServerAddress address, ushort pingMs, bool isPingMeasurementDone)
        {
            if (!isPingMeasurementDone)
            {
                return;
            }

            foreach (var serversList in this.allServersLists)
            {
                if (serversList.IsActive
                    && serversList.Controller.SortType == ServersListSortType.Ping)
                {
                    serversList.ScheduleSortEntries();
                }
            }
        }
    }
}