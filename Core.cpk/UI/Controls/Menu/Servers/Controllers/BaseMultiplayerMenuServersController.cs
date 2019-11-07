namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data;
    using AtomicTorch.CBND.GameApi.ServicesClient.Servers;

    public abstract class BaseMultiplayerMenuServersController : IDisposable
    {
        protected readonly List<(ServerAddress server, ViewModelServerInfoListEntry viewModel)>
            ServerAddressToServerViewModel =
                new List<(ServerAddress, ViewModelServerInfoListEntry)>();

        protected readonly ServerViewModelsProvider ServerViewModelsProvider;

        private bool isListAvailable;

        private ServersListSortType sortType;

        protected BaseMultiplayerMenuServersController(ServerViewModelsProvider serverViewModelsProvider)
        {
            this.ServerViewModelsProvider = serverViewModelsProvider;
        }

        public event Action IsListAvailableChanged;

        public event Action ListChanged;

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

        public ObservableCollection<ViewModelServerInfoListEntry> ServersCollection { get; }
            = new ObservableCollection<ViewModelServerInfoListEntry>();

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
                this.FillServersList();
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

        protected void FillServersList()
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

        protected IEnumerable<ViewModelServerInfoListEntry> GetOrderedList()
        {
            var entries = this.ServerAddressToServerViewModel.Select(s => s.viewModel);
            if (this.sortType == ServersListSortType.None)
            {
                return entries;
            }

            return entries
                   // official first (yes, it should use negated bool flag)
                   .OrderBy(s => !s.ViewModelServerInfo.IsOfficial)
                   .ThenBy(s => !s.ViewModelServerInfo.IsFeatured)
                   .ThenBy(s => s.ViewModelServerInfo.Title);

            // TODO: implement sort controls and use the code below
            switch (this.sortType)
            {
                case ServersListSortType.None:
                    return entries;

                case ServersListSortType.Title:
                    return
                        entries
                            .Where(s => !string.IsNullOrEmpty(s.ViewModelServerInfo.Title))
                            .OrderBy(s => s.ViewModelServerInfo.Title)
                            .Concat(
                                entries
                                    .Where(s => string.IsNullOrEmpty(s.ViewModelServerInfo.Title))
                                    .OrderBy(p => p.ViewModelServerInfo.Address));

                case ServersListSortType.Ping:
                    return
                        entries
                            .Where(v => v.ViewModelServerInfo.IsPingMeasurementDone)
                            .OrderBy(s => s.ViewModelServerInfo.Ping)
                            .ThenBy(s => s.ViewModelServerInfo.Title)
                            .Concat(
                                entries
                                    .Where(v => !v.ViewModelServerInfo.IsPingMeasurementDone)
                                    .OrderBy(p => p.ViewModelServerInfo.Address));

                case ServersListSortType.OnlinePlayersCount:
                    return
                        entries
                            .OrderByDescending(s => s.ViewModelServerInfo.PlayersText)
                            .ThenBy(s => s.ViewModelServerInfo.Ping)
                            .ThenBy(s => s.ViewModelServerInfo.Title);

                default:
                    throw new ArgumentException("Unknown sort order" + this.sortType);
            }
        }

        protected void OnListChanged()
        {
            this.ListChanged?.Invoke();
        }

        protected abstract void PopulateServersList();

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