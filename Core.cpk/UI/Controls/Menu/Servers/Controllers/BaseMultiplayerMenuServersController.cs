namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Servers;
    using AtomicTorch.GameEngine.Common.Extensions;

    public abstract class BaseMultiplayerMenuServersController : IDisposable
    {
        private const string ServerOfflineTitle
            = "[" + ServerViewModelsProvider.InfoServerOfflineTitle + "]";

        protected readonly List<(ServerAddress server, ViewModelServerInfoListEntry viewModel)>
            ServerAddressToServerViewModel =
                new();

        protected readonly ServerViewModelsProvider ServerViewModelsProvider;

        private ServersListSortType defaultSortType;

        private bool isListAvailable;

        private bool isSortOrderReversed;

        private bool isSortScheduled;

        private ServersListSortType sortType;

        protected BaseMultiplayerMenuServersController(ServerViewModelsProvider serverViewModelsProvider)
        {
            this.ServerViewModelsProvider = serverViewModelsProvider;
            ClientUpdateHelper.UpdateCallback += this.Update;
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
                this.ScheduleSortEntries();
            }
        }

        public SuperObservableCollection<ViewModelServerInfoListEntry> ServersCollection { get; }
            = new();

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
                this.ScheduleSortEntries();
            }
        }

        public virtual void Dispose()
        {
            this.RemoveAllViewModels();
            this.ServersCollection.Clear();
            ClientUpdateHelper.UpdateCallback -= this.Update;
        }

        public void RebuildServersList()
        {
            this.RemoveAllViewModels();
            this.PopulateServersList();
            this.FillServersList();
        }

        public abstract void ReloadServersList();

        public void ScheduleSortEntries()
        {
            this.isSortScheduled = true;
        }

        protected IEnumerable<ViewModelServerInfoListEntry> GetOrderedList()
        {
            var entries = this.ServerAddressToServerViewModel.Select(s => s.viewModel);
            if (this.sortType == ServersListSortType.None)
            {
                return entries; // return as is (it could be a recent/history list)
            }

            entries = this.sortType switch
            {
                ServersListSortType.Featured
                    => ReverseIfRequested(
                        entries.OrderBy(s => !s.ViewModelServerInfo.IsOfficial) // official first
                               .ThenBy(s => !s.ViewModelServerInfo.IsFeatured)  // then featured
                               .ThenBy(s => s.ViewModelServerInfo.Title)),

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

            // apply further sorting to return incompatible servers last
            return entries.OrderBy(s => !s.ViewModelServerInfo.IsInfoReceived
                                        || s.ViewModelServerInfo.IsCompatible == false);

            IEnumerable<ViewModelServerInfoListEntry> ReverseIfRequested(
                IEnumerable<ViewModelServerInfoListEntry> query)
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
            this.isSortScheduled = false;
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

        private void Update()
        {
            if (!this.isSortScheduled)
            {
                return;
            }

            this.isSortScheduled = false;

            using var tempOrderedList = Api.Shared.WrapInTempList(this.GetOrderedList());
            var currentList = this.ServersCollection;
            var order = tempOrderedList.AsList();
            currentList.ApplySortOrder(order);
        }
    }
}