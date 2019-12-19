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

        public void SortEntries()
        {
            using var tempOrderedList = Api.Shared.WrapInTempList(this.GetOrderedList());
            // simply reset the observable collection
            this.ServersCollection.ClearAndAddRange(tempOrderedList);
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
            return this.sortType switch
            {
                ServersListSortType.None => entries, // return as is (it could be a recent/history list)

                ServersListSortType.Featured => entries
                                                // official first (yes, it should use negated bool flag)
                                                .OrderBy(s => !s.ViewModelServerInfo.IsOfficial)
                                                .ThenBy(s => !s.ViewModelServerInfo.IsFeatured)
                                                .ThenBy(s => s.ViewModelServerInfo.Title),

                ServersListSortType.Title => entries.Where(s => !string.IsNullOrEmpty(s.ViewModelServerInfo.Title))
                                                    .OrderBy(s => s.ViewModelServerInfo.Title)
                                                    .Concat(entries
                                                            .Where(s => string.IsNullOrEmpty(
                                                                       s.ViewModelServerInfo.Title))
                                                            .OrderBy(p => p.ViewModelServerInfo.Address)),

                ServersListSortType.Ping => entries.Where(v => v.ViewModelServerInfo.IsPingMeasurementDone)
                                                   .OrderBy(s => s.ViewModelServerInfo.Ping)
                                                   .ThenBy(s => s.ViewModelServerInfo.Title)
                                                   .Concat(entries.Where(v => !v.ViewModelServerInfo
                                                                                .IsPingMeasurementDone)
                                                                  .OrderBy(p => p.ViewModelServerInfo.Address)),

                ServersListSortType.OnlinePlayersCount => entries
                                                          .OrderBy(s => !s.ViewModelServerInfo.IsInfoReceived)
                                                          .ThenByDescending(
                                                              s => s.ViewModelServerInfo.PlayersOnlineCount)
                                                          .ThenBy(s => s.ViewModelServerInfo.Title),

                _ => throw new ArgumentException("Unknown sort order: " + this.sortType)
            };
        }

        protected void OnListChanged()
        {
            this.ListChanged?.Invoke();
        }

        protected abstract void PopulateServersList();

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