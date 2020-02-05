namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Servers;

    public abstract class BaseMultiplayerMenuServersController : IDisposable
    {
        private const string ServerOfflineTitle
            = "[" + ServerViewModelsProvider.InfoServerOfflineTitle + "]";

        protected readonly List<(ServerAddress server, ViewModelServerInfoListEntry viewModel)>
            ServerAddressToServerViewModel =
                new List<(ServerAddress, ViewModelServerInfoListEntry)>();

        protected readonly ServerViewModelsProvider ServerViewModelsProvider;

        private ServersListSortType defaultSortType;

        private bool isListAvailable;

        private bool isSortOrderReversed;

        private ServersListSortType sortType;

        protected BaseMultiplayerMenuServersController(ServerViewModelsProvider serverViewModelsProvider)
        {
            this.ServerViewModelsProvider = serverViewModelsProvider;
        }

        public event Action IsListAvailableChanged;

        public event Action ListChanged;

        public ServersListSortType DefaultSortType
        {
            get => this.defaultSortType;
            set
            {
                this.defaultSortType = value;
                this.SortType = value;
            }
        }

        public bool IsClearingNow { get; set; }

        public bool IsListAvailable
        {
            get => this.isListAvailable;
            set
            {
                if (this.isListAvailable == value)
                {
                    return;
                }

                this.isListAvailable = value;
                this.IsListAvailableChanged?.Invoke();
            }
        }

        public bool IsSortOrderReversed
        {
            get => this.isSortOrderReversed;
            set
            {
                if (this.IsSortOrderReversed == value)
                {
                    return;
                }

                this.isSortOrderReversed = value;
                this.SortEntries();
            }
        }

        public SuperObservableCollection<ViewModelServerInfoListEntry> ServersCollection { get; }
            = new SuperObservableCollection<ViewModelServerInfoListEntry>();

        public ServersListSortType SortType
        {
            get => this.sortType;
            set
            {
                if (this.sortType == value)
                {
                    return;
                }

                this.sortType = value;
                this.SortEntries();
            }
        }

        public virtual void Dispose()
        {
            this.RemoveAllViewModels();
            this.ServersCollection.Clear();
        }

        public void RebuildServersList()
        {
            this.RemoveAllViewModels();
            this.PopulateServersList();
            this.FillServersList();
        }

        public abstract void ReloadServersList();

        public void SortEntries()
        {
            using var tempOrderedList = Api.Shared.WrapInTempList(this.GetOrderedList());
            // simply reset the observable collection
            this.ServersCollection.ClearAndAddRange(tempOrderedList.AsList());
            return;

            // not implemented by NoesisGUI
            var currentList = this.ServersCollection;
            var order = tempOrderedList.AsList();
            for (var index = 0; index < order.Count; index++)
            {
                var currentIndex = currentList.IndexOf(order[index]);
                if (currentIndex == index)
                {
                    continue;
                }

                currentList.Move(currentIndex, index);
                Api.Logger.Dev("Moving: " + currentIndex + " -> " + index);
            }
        }

        protected IEnumerable<ViewModelServerInfoListEntry> GetOrderedList()
        {
            var entries = this.ServerAddressToServerViewModel.Select(s => s.viewModel);
            if (this.sortType == ServersListSortType.None)
            {
                return entries; // return as is (it could be a recent/history list)
            }

            if (this.sortType == ServersListSortType.Featured)
            {
                entries = ReverseIfRequested(
                    entries.OrderBy(s => !s.ViewModelServerInfo.IsOfficial) // official first
                           .ThenBy(s => !s.ViewModelServerInfo.IsFeatured)  // then featured
                           .ThenBy(s => s.ViewModelServerInfo.Title));

                // apply further sorting to return incompatible servers last
                return entries.OrderBy(s => s.ViewModelServerInfo.IsCompatible == false);
            }

            return this.sortType switch
            {
                ServersListSortType.Title
                => ReverseIfRequested(entries.Where(s => !IsEmptyOrPlaceholderServerTitle(s))
                                             .OrderBy(s => s.ViewModelServerInfo.Title))
                    .Concat(entries.Where(IsEmptyOrPlaceholderServerTitle)
                                   .OrderBy(p => p.ViewModelServerInfo.Address)),

                ServersListSortType.Ping
                => ReverseIfRequested(
                        entries.Where(v => v.ViewModelServerInfo.Ping.HasValue)
                               .OrderBy(s => s.ViewModelServerInfo.Ping.Value)
                               .ThenBy(s => s.ViewModelServerInfo.Title))
                    .Concat(entries.Where(s => !s.ViewModelServerInfo.Ping.HasValue)
                                   .OrderBy(s => s.ViewModelServerInfo.Title)),

                ServersListSortType.OnlinePlayersCount
                => ReverseIfRequested(
                        entries.Where(s => s.ViewModelServerInfo.IsInfoReceived)
                               .OrderByDescending(s => s.ViewModelServerInfo.PlayersOnlineCount)
                               .ThenBy(s => s.ViewModelServerInfo.Title))
                    .Concat(entries.Where(s => !s.ViewModelServerInfo.IsInfoReceived)
                                   .OrderBy(s => s.ViewModelServerInfo.Title)),

                ServersListSortType.LastWipe
                => ReverseIfRequested(
                        entries.Where(s => s.ViewModelServerInfo.IsInfoReceived)
                               .OrderByDescending(s => s.ViewModelServerInfo.WipedDate.Value)
                               .ThenBy(s => s.ViewModelServerInfo.Title))
                    .Concat(entries.Where(s => !s.ViewModelServerInfo.IsInfoReceived)
                                   .OrderBy(s => s.ViewModelServerInfo.Title)),

                _ => throw new ArgumentException("Unknown sort order: " + this.sortType)
            };

            IEnumerable<ViewModelServerInfoListEntry> ReverseIfRequested(IEnumerable<ViewModelServerInfoListEntry> query)
            {
                if (this.IsSortOrderReversed)
                {
                    query = query.Reverse();
                }

                return query;
            }
        }

        protected void OnListChanged()
        {
            this.ListChanged?.Invoke();
        }

        protected abstract void PopulateServersList();

        private static bool IsEmptyOrPlaceholderServerTitle(ViewModelServerInfoListEntry s)
        {
            var title = s.ViewModelServerInfo.Title;
            return string.IsNullOrEmpty(title)
                   || "...".Equals(title, StringComparison.Ordinal)
                   || ServerOfflineTitle.Equals(title, StringComparison.Ordinal);
        }

        private void FillServersList()
        {
            this.IsClearingNow = true;
            this.ServersCollection.Clear();
            foreach (var server in this.GetOrderedList())
            {
                this.ServersCollection.Add(server);
            }

            this.IsClearingNow = false;
            this.OnListChanged();
        }

        private void RemoveAllViewModels()
        {
            foreach (var server in this.ServerAddressToServerViewModel)
            {
                this.ServerViewModelsProvider.ReturnServerInfoViewModel(server.viewModel);
            }

            this.ServerAddressToServerViewModel.Clear();
        }
    }
}