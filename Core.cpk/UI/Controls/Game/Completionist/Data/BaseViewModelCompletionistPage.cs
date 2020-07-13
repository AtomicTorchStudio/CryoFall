namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Completionist.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Systems.Completionist;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core.Menu;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;

    public abstract class BaseViewModelCompletionistPage<TDataEntry, TViewDataEntry>
        : BaseViewModel
        where TDataEntry : struct, ICompletionistDataEntry
        where TViewDataEntry : ViewDataEntryCompletionist
    {
        private readonly Dictionary<IProtoEntity, TViewDataEntry> binding;

        private readonly Action entriesPendingCountChanged;

        private TDataEntry lastRemovedEntryByServer;

        private int lastRemovedIndex;

        private int pendingEntriesCount;

        private NetworkSyncList<TDataEntry> source;

        protected BaseViewModelCompletionistPage(
            Dictionary<IProtoEntity, TViewDataEntry> binding,
            int columnsCount,
            int iconSize,
            Action entriesPendingCountChanged)
        {
            this.binding = binding;
            this.entriesPendingCountChanged = entriesPendingCountChanged;
            this.ColumnsCount = columnsCount;
            this.IconSize = iconSize;
        }

        public int ClaimedEntriesCount { get; private set; }

        public int ColumnsCount { get; }

        public SuperObservableCollection<TViewDataEntry> Entries { get; }
            = new SuperObservableCollection<TViewDataEntry>();

        public int EntriesTotalCount => this.binding.Count;

        public bool HasPendingEntries { get; private set; }

        public int IconSize { get; }

        public int PendingEntriesCount
        {
            get => this.pendingEntriesCount;
            private set
            {
                if (this.pendingEntriesCount == value)
                {
                    return;
                }

                this.pendingEntriesCount = value;
                this.NotifyThisPropertyChanged();
                this.entriesPendingCountChanged?.Invoke();
            }
        }

        public string ProgressText => this.ClaimedEntriesCount + "/" + this.EntriesTotalCount;

        protected internal NetworkSyncList<TDataEntry> Source
        {
            get => this.source;
            set
            {
                if (ReferenceEquals(this.source, value))
                {
                    return;
                }

                if (this.source != null)
                {
                    this.source.ClientElementInserted -= this.SourceListElementInsertedHandler;
                    this.source.ClientElementRemoved -= this.SourceListElementRemovedHandler;
                }

                this.source = value;

                if (this.source is null)
                {
                    this.Entries.Clear();
                    return;
                }

                this.source.ClientElementInserted += this.SourceListElementInsertedHandler;
                this.source.ClientElementRemoved += this.SourceListElementRemovedHandler;

                // setup new entries list
                var list = new List<TViewDataEntry>();
                var entriesRewardAvailable = new HashSet<IProtoEntity>();
                var entriesRewardClaimed = new HashSet<IProtoEntity>();

                foreach (var sourceEntry in this.source.OrderBy(e => e.Prototype.Name,
                                                                StringComparer.InvariantCultureIgnoreCase))
                {
                    var prototype = sourceEntry.Prototype;
                    var viewModel = this.binding[prototype];

                    if (sourceEntry.IsRewardClaimed)
                    {
                        entriesRewardClaimed.Add(prototype);
                        this.RefreshViewModelState(sourceEntry, viewModel);
                        continue;
                    }

                    viewModel.State = ViewModelWindowCompletionist.CompletionistEntryState.RewardAvailable;
                    list.Add(viewModel);
                    entriesRewardAvailable.Add(prototype);
                }

                foreach (var pair in this.binding.OrderBy(e => e.Key.Name,
                                                          StringComparer.InvariantCultureIgnoreCase))
                {
                    if (entriesRewardAvailable.Contains(pair.Key))
                    {
                        // already added
                        continue;
                    }

                    var viewModel = pair.Value;
                    viewModel.State = entriesRewardClaimed.Contains(pair.Key)
                                          ? ViewModelWindowCompletionist.CompletionistEntryState.RewardClaimed
                                          : ViewModelWindowCompletionist.CompletionistEntryState.Undiscovered;
                    list.Add(viewModel);
                }

                this.Entries.ClearAndAddRange(list);
                this.RefreshProgress();
            }
        }

        protected virtual void RefreshViewModelState(TDataEntry value, TViewDataEntry viewModel)
        {
            viewModel.State = value.IsRewardClaimed
                                  ? ViewModelWindowCompletionist.CompletionistEntryState.RewardClaimed
                                  : ViewModelWindowCompletionist.CompletionistEntryState.RewardAvailable;
        }

        private void RefreshProgress()
        {
            this.ClaimedEntriesCount =
                this.Entries.Count(e => e.State == ViewModelWindowCompletionist.CompletionistEntryState.RewardClaimed);
            this.PendingEntriesCount =
                this.Entries.Count(e => e.State
                                        == ViewModelWindowCompletionist.CompletionistEntryState.RewardAvailable);
            this.HasPendingEntries = this.pendingEntriesCount > 0;
            this.NotifyPropertyChanged(nameof(this.ProgressText));
        }

        private void RemoveEntry(IProtoEntity prototype)
        {
            for (var index = 0; index < this.Entries.Count; index++)
            {
                var entry = this.Entries[index];
                if (!ReferenceEquals(entry.Prototype, prototype))
                {
                    continue;
                }

                this.Entries.RemoveAt(index);
                this.lastRemovedIndex = index;
                return;
            }
        }

        private void SourceListElementInsertedHandler(
            NetworkSyncList<TDataEntry> source,
            int index,
            TDataEntry value)
        {
            var prototype = value.Prototype;
            this.RemoveEntry(prototype);

            var viewModel = this.binding[prototype];
            this.RefreshViewModelState(value, viewModel);

            var insertionIndex = 0;
            if (value.IsRewardClaimed)
            {
                // insert in place of the last removed entry (if there were any removal, consider it's as a replacement of entry)
                // otherwise just insert as first entry
                if (this.lastRemovedIndex >= this.Entries.Count)
                {
                    this.lastRemovedIndex = 0;
                }

                insertionIndex = this.lastRemovedIndex;
            }

            this.Entries.Insert(insertionIndex, viewModel);
            this.lastRemovedIndex = 0;

            this.RefreshProgress();

            try
            {
                if (viewModel.State == ViewModelWindowCompletionist.CompletionistEntryState.RewardAvailable)
                {
                    if (ReferenceEquals(this.lastRemovedEntryByServer.Prototype, prototype))
                    {
                        return;
                    }

                    NotificationSystem.ClientShowNotification(
                        viewModel.Title,
                        viewModel.NotificationMessage,
                        icon: viewModel.IconTexture,
                        color: NotificationColor.Good,
                        onClick: Menu.Open<WindowCompletionist>);
                    return;
                }

                if (viewModel.State == ViewModelWindowCompletionist.CompletionistEntryState.RewardClaimed
                    && viewModel is ViewDataEntryFishCompletionist viewModelFish)
                {
                    if (ReferenceEquals(this.lastRemovedEntryByServer.Prototype, prototype)
                        && !this.lastRemovedEntryByServer.IsRewardClaimed)
                    {
                        // reward was not claimed but now claimed - no need to display the "new record" notification
                        return;
                    }

                    // ReSharper disable once CanExtractXamlLocalizableStringCSharp
                    NotificationSystem.ClientShowNotification(
                        CoreStrings.WindowCompletionist_Notification_FishNewRecord,
                        string.Format("{0}[br]{1}[br]{2}",
                                      viewModel.Title,
                                      string.Format(CoreStrings.WindowCompletionist_BestFishWeight_Format,
                                                    viewModelFish.MaxWeightText),
                                      string.Format(CoreStrings.WindowCompletionist_BestFishLength_Format,
                                                    viewModelFish.MaxLengthText)),
                        icon: viewModel.IconTexture,
                        color: NotificationColor.Good,
                        onClick: Menu.Open<WindowCompletionist>);
                }
            }
            finally
            {
                this.lastRemovedEntryByServer = default;
            }
        }

        private void SourceListElementRemovedHandler(
            NetworkSyncList<TDataEntry> networkSyncList,
            int removedAtIndex,
            TDataEntry removedValue)
        {
            var prototype = removedValue.Prototype;
            this.RemoveEntry(prototype);
            this.RefreshProgress();

            this.lastRemovedEntryByServer = removedValue;
        }
    }
}