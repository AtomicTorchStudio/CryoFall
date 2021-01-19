namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Faction.Data
{
    using System;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Politics;
    using AtomicTorch.CBND.CoreMod.UI.Helpers;

    public class ViewModelFactionsListControl : BaseViewModel
    {
        private string filterByClanTag;

        private bool includePrivateFactions = true;

        private bool includePublicFactions = true;

        private bool isActive;

        private FactionListSortOrder selectedSortOrder;

        public ViewModelFactionsListControl(FactionListControlMode mode)
        {
            this.Mode = mode;
            this.SortOrders = EnumHelper.EnumValuesToViewModel<FactionListSortOrder>();
            FactionSystem.ClientCurrentFactionChanged += this.CurrentFactionChangedHandler;
            this.SetDefaultFilters();
        }

        public SuperObservableCollection<ViewModelFactionEntry> FactionsList { get; }
            = new();

        public string FilterByClanTag
        {
            get => this.filterByClanTag;
            set
            {
                if (this.filterByClanTag == value)
                {
                    return;
                }

                this.filterByClanTag = value;
                this.NotifyThisPropertyChanged();

                this.Refresh();
            }
        }

        public bool IncludePrivateFactions
        {
            get => this.includePrivateFactions;
            set
            {
                if (this.includePrivateFactions == value)
                {
                    return;
                }

                this.includePrivateFactions = value;
                this.NotifyThisPropertyChanged();

                if (!this.includePrivateFactions
                    && !this.includePublicFactions)
                {
                    this.IncludePublicFactions = true;
                }

                this.Refresh();
            }
        }

        public bool IncludePublicFactions
        {
            get => this.includePublicFactions;
            set
            {
                if (this.includePublicFactions == value)
                {
                    return;
                }

                this.includePublicFactions = value;
                this.NotifyThisPropertyChanged();

                if (!this.includePublicFactions
                    && !this.includePublicFactions)
                {
                    this.IncludePrivateFactions = true;
                }

                this.Refresh();
            }
        }

        public bool IsActive
        {
            get => this.isActive;
            set
            {
                if (this.isActive == value)
                {
                    return;
                }

                this.isActive = value;
                this.NotifyThisPropertyChanged();

                if (this.IsActive)
                {
                    this.Refresh();
                }
                else
                {
                    this.FactionsList.Clear();
                    this.SetDefaultFilters();
                }
            }
        }

        public bool IsFiltersVisible => this.Mode == FactionListControlMode.AllFactionsList;

        public bool IsLoading { get; private set; } = true;

        public FactionListControlMode Mode { get; }

        public FactionListSortOrder SelectedSortOrder
        {
            get => this.selectedSortOrder;
            set
            {
                if (this.selectedSortOrder == value)
                {
                    return;
                }

                this.selectedSortOrder = value;
                this.NotifyThisPropertyChanged();

                this.Refresh();
            }
        }

        public ViewModelEnum<FactionListSortOrder>[] SortOrders { get; }

        public async void Refresh()
        {
            if (!this.isActive)
            {
                return;
            }

            this.FactionsList.Clear();
            this.IsLoading = true;

            ushort page = 0;
            var pageSize = FactionSystem.FactionListPageSizeMin;
            var sortOrder = this.selectedSortOrder;
            var clanTagFilter = this.FilterByClanTag?.Trim();

            FactionKind? kindFilter = null;

            if (this.includePrivateFactions
                && this.includePublicFactions)
            {
                // no filter
            }
            else if (this.includePrivateFactions)
            {
                kindFilter = FactionKind.Private;
            }
            else if (this.includePublicFactions)
            {
                kindFilter = FactionKind.Public;
            }

            FactionSystem.FactionListFilter factionListFilter;
            switch (this.Mode)
            {
                case FactionListControlMode.Leaderboard:
                    factionListFilter = FactionSystem.FactionListFilter.Leaderboard;
                    sortOrder = FactionListSortOrder.LeaderboardRank;
                    break;

                case FactionListControlMode.AllFactionsList:
                    factionListFilter = FactionSystem.FactionListFilter.AllFactions;
                    break;

                case FactionListControlMode.InvitationsList:
                    factionListFilter = FactionSystem.FactionListFilter.OnlyWithReceivedInvitations;
                    break;

                case FactionListControlMode.ApplicationsList:
                    factionListFilter = FactionSystem.FactionListFilter.OnlyWithSubmittedApplications;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            var result = await FactionSystem.ClientGetFactionsList(page,
                                                                   pageSize,
                                                                   sortOrder,
                                                                   clanTagFilter,
                                                                   kindFilter,
                                                                   factionListFilter);
            if (this.IsDisposed)
            {
                return;
            }

            this.IsLoading = false;
            var newItems = new ViewModelFactionEntry[result.Entries.Length];
            for (var index = 0; index < result.Entries.Length; index++)
            {
                newItems[index] = new ViewModelFactionEntry(
                    result.Entries[index],
                    isLeaderboardEntry: this.Mode == FactionListControlMode.Leaderboard,
                    isPreviewEntry: true);
            }

            this.FactionsList.ClearAndAddRange(newItems);
        }

        protected override void DisposeViewModel()
        {
            this.IsActive = false;
            FactionSystem.ClientCurrentFactionChanged -= this.CurrentFactionChangedHandler;
            base.DisposeViewModel();
        }

        private void CurrentFactionChangedHandler()
        {
            this.FactionsList.Clear();
        }

        private void SetDefaultFilters()
        {
            this.IncludePrivateFactions = true;
            this.IncludePublicFactions = true;
            this.FilterByClanTag = default;

            switch (this.Mode)
            {
                case FactionListControlMode.Leaderboard:
                    this.SelectedSortOrder = FactionListSortOrder.LeaderboardRank;
                    this.IncludePublicFactions = false;
                    break;

                default:
                    this.SelectedSortOrder = FactionListSortOrder.MembersNumber;
                    break;
            }

            this.Refresh();
        }
    }
}