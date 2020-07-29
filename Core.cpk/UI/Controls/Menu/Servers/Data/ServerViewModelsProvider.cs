namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Servers;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.DataStructures;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Primitives;

    /// <summary>
    /// Provides ViewModelServerInfo for the server address.
    /// </summary>
    public class ServerViewModelsProvider
    {
        public const string DialogServerMods_MessageNoMods = "No mods used on this server.";

        public const string DialogServerMods_Title = "Server mods";

        public const string InfoServerNotAccessibleTitle = "The server is not accessible.";

        public const string InfoServerOfflineTitle = "Server offline";

        private static readonly BaseCommand CommandJoinServer
            = new ActionCommandWithParameter(
                obj => JoinServerHelper.ExecuteCommandJoinServer((ViewModelServerInfo)obj));

        private static ServerViewModelsProvider instance;

        private static bool showEmptyServers = true;

        private static bool showIncompatibleServers = true;

        private static bool showPvEServers = true;

        private static bool showPvPServers = true;

        private readonly BaseCommand commandRefresh;

        private readonly IServersProvider serversProvider = Api.Client.MasterServer.ServersProvider;

        private readonly ListDictionary<ServerAddress, ViewModelServerInfo> serverViewModels
            = new ListDictionary<ServerAddress, ViewModelServerInfo>();

        private bool isEnabled;

        private ServerViewModelsProvider()
        {
            this.commandRefresh = new ActionCommandWithParameter(
                viewModelServerInfo => this.ExecuteCommandRefresh((ViewModelServerInfo)viewModelServerInfo,
                                                                  forceReset: true));

            this.serversProvider.ServerCannotConnect += this.ServerCannotConnectHandler;
            this.serversProvider.ServerInfoReceived += this.ServerInfoReceivedHandler;
            this.serversProvider.ServerPingUpdated += this.ServerPingUpdatedHandled;
            this.serversProvider.ServerIconLoaded += this.ServerIconLoadedHandler;
            this.serversProvider.ServerPublicGuidAddressResolved += this.ServerPublicGuidAddressResolvedHandler;
            //this.serversProvider.EnableInfoConnections();
        }

        public static ServerViewModelsProvider Instance
            => instance ??= new ServerViewModelsProvider();

        public static bool ShowEmptyServers
        {
            get => showEmptyServers;
            set
            {
                if (ShowEmptyServers == value)
                {
                    return;
                }

                showEmptyServers = value;
                foreach (var pair in Instance.serverViewModels)
                {
                    pair.Value.RefreshVisibilityInList();
                }
            }
        }

        public static bool ShowIncompatibleServers
        {
            get => showIncompatibleServers;
            set
            {
                if (showIncompatibleServers == value)
                {
                    return;
                }

                showIncompatibleServers = value;
                foreach (var pair in Instance.serverViewModels)
                {
                    pair.Value.RefreshVisibilityInList();
                }
            }
        }

        public static bool ShowPvEServers
        {
            get => showPvEServers;
            set
            {
                if (showPvEServers == value)
                {
                    return;
                }

                showPvEServers = value;
                foreach (var pair in Instance.serverViewModels)
                {
                    pair.Value.RefreshVisibilityInList();
                }
            }
        }

        public static bool ShowPvPServers
        {
            get => showPvPServers;
            set
            {
                if (showPvPServers == value)
                {
                    return;
                }

                showPvPServers = value;
                foreach (var pair in Instance.serverViewModels)
                {
                    pair.Value.RefreshVisibilityInList();
                }
            }
        }

        public bool IsEnabled
        {
            get => this.isEnabled;
            set
            {
                if (this.isEnabled == value)
                {
                    return;
                }

                this.isEnabled = value;

                if (this.isEnabled)
                {
                    Api.Client.MasterServer.ServersProvider.EnableInfoConnections();
                    this.RefreshAll();
                }
                else
                {
                    Api.Client.MasterServer.ServersProvider.DisableInfoConnections();
                }
            }
        }

        public ViewModelServerInfoListEntry GetServerInfoViewModel(ServerAddress address)
        {
            ViewModelServerInfo viewModelServerInfo = null;
            if (address.PublicGuid != AtomicGuid.Empty)
            {
                // search by GUID
                foreach (var serverViewModel in this.serverViewModels)
                {
                    if (serverViewModel.Key.PublicGuid != address.PublicGuid)
                    {
                        continue;
                    }

                    // found!
                    viewModelServerInfo = serverViewModel.Value;
                    if (!string.IsNullOrEmpty(address.HostAddress))
                    {
                        // ensure the correct host address is used
                        viewModelServerInfo.UpdateAddress(address);
                    }

                    break;
                }
            }
            else if (!string.IsNullOrEmpty(address.HostAddress))
            {
                // search by host address
                foreach (var serverViewModel in this.serverViewModels)
                {
                    if (address.HostAddress.Equals(serverViewModel.Key.HostAddress, StringComparison.OrdinalIgnoreCase))
                    {
                        viewModelServerInfo = serverViewModel.Value;
                        break;
                    }
                }
            }
            else
            {
                throw new Exception("Impossible");
            }

            if (viewModelServerInfo == null)
            {
                viewModelServerInfo = new ViewModelServerInfo(
                    address,
                    isFavorite: this.serversProvider.Favorite.Contains(address),
                    commandFavoriteToggle: new ActionCommandWithParameter(this.ExecuteCommandFavoriteToggle),
                    commandDisplayModsInfo: new ActionCommandWithParameter(this.ExecuteCommandDisplayModsInfo),
                    commandJoinServer: CommandJoinServer);

                this.serverViewModels[address] = viewModelServerInfo;

                if (this.serversProvider.AreInfoConnectionsEnabled)
                {
                    if (!string.IsNullOrEmpty(address.HostAddress))
                    {
                        this.serversProvider.RequestServerInfo(address);
                    }
                    else if (address.PublicGuid != AtomicGuid.Empty)
                    {
                        this.serversProvider.RequestResolveServerPublicGuid(address.PublicGuid);
                    }
                    else
                    {
                        // impossible
                    }
                }
            }

            viewModelServerInfo.ReferencesCount++;

            return new ViewModelServerInfoListEntry(viewModelServerInfo);
        }

        public void RefreshAll()
        {
            foreach (var server in this.serverViewModels)
            {
                this.ExecuteCommandRefresh(server.Value, forceReset: true);
            }
        }

        public void ReturnServerInfoViewModel(ViewModelServerInfoListEntry viewModelServerListEntry)
        {
            var viewModelServerInfo = viewModelServerListEntry.ViewModelServerInfo;
            viewModelServerListEntry.Dispose();

            if (this.serverViewModels.Find(viewModelServerInfo.Address)
                != viewModelServerInfo)
            {
                Api.Logger.Error("Somebody created another ViewModelServerInfo for the same server address? "
                                 + viewModelServerInfo.Address);
                return;
            }

            viewModelServerInfo.ReferencesCount--;
            if (viewModelServerInfo.ReferencesCount > 0)
            {
                // still used somewhere
                return;
            }

            // all list entry view models returned - can destroy the view model for the server info
            // ReSharper disable once AccessToDisposedClosure
            this.serverViewModels.Remove(viewModelServerInfo.Address);
            this.serversProvider.StopGettingServerInfo(viewModelServerInfo.Address);
            viewModelServerInfo.Dispose();
        }

        public void ServerIconLoadedHandler(string iconHash)
        {
            foreach (var pair in this.serverViewModels)
            {
                var viewModelServer = pair.Value;
                if (viewModelServer.IconHash != iconHash)
                {
                    continue;
                }

                viewModelServer.ReloadIcon();
            }
        }

        [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
        public void ServerInfoReceivedHandler(ServerInfo serverInfo)
        {
            var viewModel = this.serverViewModels.Find(serverInfo.ServerAddress);
            if (viewModel == null)
            {
                // unknown server
                return;
            }

            viewModel.Title = serverInfo.ServerName;
            viewModel.Description = serverInfo.Description;

            viewModel.Version = serverInfo.ServerVersion;
            viewModel.IsCompatible = serverInfo.ServerVersion.WithoutBuildNumber
                                     == Api.Shared.GameVersionNumber.WithoutBuildNumber;

            viewModel.NetworkProtocolVersion = serverInfo.ServerNetworkProtocolVersion;
            viewModel.PlayersText = $"{serverInfo.PlayersOnlineCount}/{serverInfo.PlayersMaxCount}";
            viewModel.PlayersOnlineCount = serverInfo.PlayersOnlineCount;
            viewModel.IconHash = serverInfo.IconHash;
            viewModel.ModsOnServer = serverInfo.ModsOnServer;
            viewModel.IsPvP = serverInfo.ScriptingTags.Contains("PvP", StringComparer.Ordinal);
            viewModel.IsPvE = serverInfo.ScriptingTags.Contains("PvE", StringComparer.Ordinal);
            viewModel.WipedDate = serverInfo.CreationDateUtc.ToLocalTime();

            viewModel.IsInfoReceived = true;
            viewModel.RefreshVisibilityInList();
        }

        public void ServerPingUpdatedHandled(ServerAddress address, ushort pingMs, bool isPingMeasurementDone)
        {
            var viewModelServer = this.serverViewModels.Find(address);
            if (viewModelServer == null)
            {
                return;
            }

            viewModelServer.Ping = pingMs;
            viewModelServer.CommandRefresh ??= this.commandRefresh;

            if (isPingMeasurementDone)
            {
                viewModelServer.IsPingMeasurementDone = true;
                this.ScheduleAutoRefresh(viewModelServer);
            }
        }

        public void SetFavorite(ServerAddress address, bool isFavorite)
        {
            Api.Logger.Info($"Server {address} marked as {(isFavorite ? "favorite" : "non favorite")}");

            var viewModelServer = this.serverViewModels.Find(address);
            if (viewModelServer == null)
            {
                return;
            }

            var serversFavorite = this.serversProvider.Favorite;
            if (isFavorite)
            {
                serversFavorite.Add(address);
            }
            else
            {
                serversFavorite.Remove(address);
            }

            serversFavorite.Save();
        }

        private void ExecuteCommandDisplayModsInfo(object obj)
        {
            var serverInfo = (ViewModelServerInfo)obj;
            if (!serverInfo.IsInfoReceived)
            {
                serverInfo.RefreshAndDisplayPleaseWaitDialog(
                    onInfoReceivedOrCannotReach: () => this.ExecuteCommandDisplayModsInfo(obj));
                return;
            }

            var modsOnServer = serverInfo.ModsOnServer;

            var stackPanel = new StackPanel()
            {
                Orientation = Orientation.Vertical
            };
            var stackPanelChildren = stackPanel.Children;

            var sb = new StringBuilder();
            var isNotFirst = false;
            foreach (var serverModInfo in modsOnServer)
            {
                // ReSharper disable once CanExtractXamlLocalizableStringCSharp
                if (serverModInfo.Id.Equals("core", StringComparison.Ordinal))
                {
                    continue;
                }

                if (isNotFirst)
                {
                    sb.AppendLine();
                    sb.AppendLine();
                }
                else
                {
                    isNotFirst = true;
                }

                // ReSharper disable once CanExtractXamlLocalizableStringCSharp
                sb.AppendFormat(
                    "* {0}\n  - {1} v{2} ({3})",
                    serverModInfo.Title,
                    serverModInfo.Id,
                    serverModInfo.Version,
                    this.GetModTypeString(serverModInfo.Type));
            }

            if (sb.Length == 0)
            {
                sb.Append(DialogServerMods_MessageNoMods);
            }

            var textBlock = new TextBlock()
            {
                Text = sb.ToString(),
                TextWrapping = TextWrapping.Wrap
            };

            stackPanelChildren.Add(textBlock);

            var scrollViewer = new ScrollViewer();
            ScrollViewer.SetVerticalScrollBarVisibility(scrollViewer, ScrollBarVisibility.Auto);
            scrollViewer.MaxWidth = 600;
            scrollViewer.Content = stackPanel;

            DialogWindow.ShowDialog(
                DialogServerMods_Title,
                scrollViewer,
                () => { },
                cancelAction: null,
                closeByEscapeKey: true);
        }

        private void ExecuteCommandFavoriteToggle(object obj)
        {
            var serverInfo = (ViewModelServerInfo)obj;
            this.SetFavorite(serverInfo.Address, serverInfo.IsFavorite);
        }

        private void ExecuteCommandRefresh(ViewModelServerInfo viewModelServerInfo, bool forceReset)
        {
            if (viewModelServerInfo == null
                || !this.IsEnabled)
            {
                return;
            }

            var address = viewModelServerInfo.Address;
            if (string.IsNullOrEmpty(address.HostAddress))
            {
                // don't have the resolved host address - request resolve
                this.serversProvider.RequestResolveServerPublicGuid(address.PublicGuid);
                return;
            }

            // double-lookup to ensure view model is actual
            var viewModelServer = this.serverViewModels.Find(address);
            if (viewModelServer == null)
            {
                return;
            }

            if (forceReset)
            {
                viewModelServer.Reset();
            }

            this.serversProvider.RefreshServerInfo(address);
        }

        private string GetModTypeString(ServerModInfo.GameModType type)
        {
            // TODO: return proper mod type string
            return type.ToString();
        }

        private void ScheduleAutoRefresh(ViewModelServerInfo viewModelServer)
        {
            var autoRefreshRequestId = ++viewModelServer.AutoRefreshRequestId;
            ClientTimersSystem.AddAction(
                delaySeconds: viewModelServer.IsInaccessible
                                  ? 6
                                  : 15,
                () =>
                {
                    if (autoRefreshRequestId != viewModelServer.AutoRefreshRequestId)
                    {
                        return;
                    }

                    //Api.Logger.Info("Refreshing game server info after delay: " + viewModelServer.Address);
                    this.ExecuteCommandRefresh(viewModelServer,
                                               forceReset: viewModelServer.IsInaccessible);
                });
        }

        private void ServerCannotConnectHandler(ServerAddress address)
        {
            var viewModelServer = this.serverViewModels.Find(address);
            if (viewModelServer == null)
            {
                return;
            }

            viewModelServer.Reset();
            viewModelServer.Description = InfoServerNotAccessibleTitle;
            viewModelServer.CommandRefresh = this.commandRefresh;
            viewModelServer.LoadingDisplayVisibility = Visibility.Collapsed;
            viewModelServer.Version = AppVersion.Zero;
            viewModelServer.IsCompatible = null;
            viewModelServer.IsInaccessible = true;

            this.ScheduleAutoRefresh(viewModelServer);
        }

        private void ServerPublicGuidAddressResolvedHandler(
            AtomicGuid guid,
            bool isSuccess,
            string hostAddress,
            bool isOfficial,
            bool isFeatured)
        {
            var serverAddress = new ServerAddress(guid, hostAddress);
            var viewModelServer = this.serverViewModels.Find(serverAddress);
            if (!isSuccess)
            {
                Api.Logger.Info("Server public GUID not resolved: " + guid);
                if (viewModelServer != null)
                {
                    viewModelServer.Reset();
                    viewModelServer.Title = "[" + InfoServerOfflineTitle + "]";
                    viewModelServer.IsInaccessible = true;
                }

                return;
            }

            if (viewModelServer == null)
            {
                Api.Logger.Info($"Server public GUID resolved but entry not found: {guid}: {hostAddress}");
                return;
            }

            Api.Logger.Info($"Server public GUID resolved: {guid}: {hostAddress}");

            // re-add view model under new server address
            this.serverViewModels.Remove(serverAddress);
            this.serverViewModels[serverAddress] = viewModelServer;
            viewModelServer.UpdateAddress(serverAddress);
            viewModelServer.Reset();

            viewModelServer.IsOfficial = isOfficial;
            viewModelServer.IsFeatured = isFeatured;

            if (this.serversProvider.AreInfoConnectionsEnabled)
            {
                this.serversProvider.RequestServerInfo(serverAddress);
            }
        }
    }
}