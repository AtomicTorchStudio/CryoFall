namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data
{
    using System;
    using System.Linq;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Controllers;
    using AtomicTorch.CBND.GameApi.ServicesClient.Servers;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ViewModelMenuServers : BaseViewModel
    {
        public const string DialogClearServerList_Message = "Are you sure you want to clear this servers list?";

        public const string DialogClearServerList_Title = "Clear server list";

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
            var serversProvider = Client.MasterServer.ServersProvider;

            this.FeaturedServers =
                new ViewModelServersList(
                        new MultiplayerMenuServersPublicController(
                            this.serverViewModelsProvider,
                            specialCondition: info => info.IsOfficial
                                                      || (info.IsFeatured && !info.IsModded)),
                        this.OnSelectedServerChanged)
                    {
                        IsActive = true
                    };

            this.CommunityServers =
                new ViewModelServersList(
                    new MultiplayerMenuServersPublicController(
                        this.serverViewModelsProvider,
                        specialCondition: info => !info.IsOfficial
                                                  && !info.IsFeatured
                                                  && !info.IsModded),
                    this.OnSelectedServerChanged);

            this.ModdedServers =
                new ViewModelServersList(
                    new MultiplayerMenuServersPublicController(
                        this.serverViewModelsProvider,
                        specialCondition: info => !info.IsOfficial
                                                  && info.IsModded),
                    this.OnSelectedServerChanged);

            this.CustomServers =
                new ViewModelServersList(
                    new MultiplayerMenuServersController(serversProvider.Custom, this.serverViewModelsProvider),
                    this.OnSelectedServerChanged);

            this.FavoriteServers =
                new ViewModelServersList(
                    new MultiplayerMenuServersController(serversProvider.Favorite, this.serverViewModelsProvider),
                    this.OnSelectedServerChanged);

            this.HistoryServers =
                new ViewModelServersList(
                    new MultiplayerMenuServersController(serversProvider.History, this.serverViewModelsProvider),
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
                                    // Hack: reset IsFavorite flag for previous favorite view models.
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
                this.SelectedServerVisibility = this.selectedServer != null ? Visibility.Visible : Visibility.Collapsed;

                //foreach (var viewModelServersList in this.allServersLists)
                //{
                //	viewModelServersList.SelectedServer = this.selectedServer;
                //}
            }
        }

        public Visibility SelectedServerVisibility { get; set; }

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

        protected override void DisposeViewModel()
        {
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

                               var serversProvider = Client.MasterServer.ServersProvider;
                               serversProvider.Custom.Add(newServerAddress);
                               serversProvider.Custom.Save();

                               // select custom servers list
                               foreach (var serversList in this.allServersLists)
                               {
                                   serversList.IsActive = serversList == this.CustomServers;
                               }

                               this.CustomServers.SelectedServer =
                                   this.CustomServers.ServersList.FirstOrDefault(
                                       s => s.ViewModelServerInfo.Address == newServerAddress);
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
    }
}