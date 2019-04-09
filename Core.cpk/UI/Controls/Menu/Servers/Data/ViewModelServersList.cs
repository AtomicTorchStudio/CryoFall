namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Controllers;

    public class ViewModelServersList : BaseViewModel
    {
        private readonly Action<ViewModelServerInfoListEntry> onSelectedServerChanged;

        private BaseMultiplayerMenuServersController controller;

        private bool isActive;

        private Visibility? loadingDisplayVisibility;

        private ViewModelServerInfoListEntry selectedServer;

        public ViewModelServersList(
            BaseMultiplayerMenuServersController controller,
            Action<ViewModelServerInfoListEntry> onSelectedServerChanged)
        {
            controller.ReloadServersList();

            this.controller = controller;
            this.onSelectedServerChanged = onSelectedServerChanged;
            this.ServersList = controller.ServersCollection;

            this.loadingDisplayVisibility = this.controller.IsListAvailable ? Visibility.Collapsed : Visibility.Visible;
            this.controller.IsListAvailableChanged += this.ControllerIsListAvailableChangedHandler;
            this.controller.ListChanged += this.ControllerListChangedHandler;

            this.RefreshListCount();
        }

#if !GAME

        public ViewModelServersList()
        {
            // design-time constructor only
            this.ServersList = new ObservableCollection<ViewModelServerInfoListEntry>(
                new List<ViewModelServerInfoListEntry>()
                {
                    new ViewModelServerInfoListEntry(),
                    new ViewModelServerInfoListEntry(),
                    new ViewModelServerInfoListEntry()
                });
        }

#endif

        public bool IsActive
        {
            get => this.isActive;
            set
            {
                if (this.isActive == value)
                {
                    return;
                }

                if (!value)
                {
                    this.SelectedServer = null;
                }

                this.isActive = value;
                this.NotifyThisPropertyChanged();
            }
        }

        public Visibility ListEmptyVisibility { get; set; } = Visibility.Collapsed;

        public Visibility ListNotEmptyVisibility { get; set; } = Visibility.Visible;

        public Visibility LoadingDisplayVisibility
        {
            get => this.loadingDisplayVisibility ?? Visibility.Collapsed;
            set
            {
                if (value == this.loadingDisplayVisibility)
                {
                    return;
                }

                this.loadingDisplayVisibility = value;
                this.RefreshListCount();
                this.NotifyThisPropertyChanged();
            }
        }

        public ViewModelServerInfoListEntry SelectedServer
        {
            get => this.selectedServer;
            set
            {
                if (this.controller.IsClearingNow)
                {
                    return;
                }

                if (this.selectedServer == value)
                {
                    return;
                }

                if (this.selectedServer != null)
                {
                    this.selectedServer.ViewModelServerInfo.IsSelected = false;
                }

                this.selectedServer = value;

                if (this.selectedServer != null)
                {
                    this.selectedServer.ViewModelServerInfo.IsSelected = true;
                }

                if (this.isActive)
                {
                    this.onSelectedServerChanged(this.selectedServer);
                }

                this.NotifyThisPropertyChanged();
            }
        }

        public Collection<ViewModelServersListSortType> SelectListSortBy
        {
            get
            {
                var result = new Collection<ViewModelServersListSortType>();
                foreach (var sortType in ViewModelServersListSortType.AllSortTypes)
                {
                    result.Add(ViewModelServersListSortType.GetViewModel(sortType));
                }

                return result;
            }
        }

        public ushort ServersCount { get; set; }

        public ObservableCollection<ViewModelServerInfoListEntry> ServersList { get; }

        public int SortBySelectedIndex
        {
            get
            {
                var index = ViewModelServersListSortType.AllSortTypes.IndexOf(this.controller.SortType);
                return index;
            }
            set
            {
                if (value == this.SortBySelectedIndex)
                {
                    return;
                }

                if (value < 0
                    || value >= ViewModelServersListSortType.AllSortTypes.Count)
                {
                    // invalid value
                    return;
                }

                var sortBy = ViewModelServersListSortType.AllSortTypes[value];
                this.controller.SortType = sortBy;
                this.NotifyThisPropertyChanged();
            }
        }

        public void ControllerListChangedHandler()
        {
            this.RefreshListCount();
            if (this.selectedServer != null
                && !this.controller.ServersCollection.Contains(this.selectedServer))
            {
                this.SelectedServer = null;
            }

            this.NotifyPropertyChanged(nameof(this.SelectedServer));
        }

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();

            if (this.controller != null)
            {
                this.controller.IsListAvailableChanged -= this.ControllerIsListAvailableChangedHandler;
                this.controller.ListChanged -= this.ControllerListChangedHandler;
                this.controller.Dispose();
                this.controller = null;
            }
        }

        private void ControllerIsListAvailableChangedHandler()
        {
            this.LoadingDisplayVisibility = this.controller.IsListAvailable
                                                ? Visibility.Collapsed
                                                : Visibility.Visible;
        }

        private void RefreshListCount()
        {
            if (this.loadingDisplayVisibility == Visibility.Visible)
            {
                // list is not yet loaded
                this.ListEmptyVisibility = Visibility.Collapsed;
                this.ListNotEmptyVisibility = Visibility.Collapsed;
                this.ServersCount = 0;
                return;
            }

            var count = this.controller.ServersCollection.Count;
            this.ListEmptyVisibility = count == 0 ? Visibility.Visible : Visibility.Collapsed;
            this.ListNotEmptyVisibility = count > 0 ? Visibility.Visible : Visibility.Collapsed;
            this.ServersCount = (ushort)count;
        }
    }
}