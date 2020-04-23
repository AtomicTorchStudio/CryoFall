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

    public class ViewModelCompletionistPage : BaseViewModel
    {
        private readonly Dictionary<IProtoEntity, ViewDataEntryCompletionist> binding;

        private readonly Action entriesPendingCountChanged;

        private int lastRemovedIndex;

        private int pendingEntriesCount;

        private NetworkSyncList<DataEntryCompletionist> source;

        public ViewModelCompletionistPage(
            Dictionary<IProtoEntity, ViewDataEntryCompletionist> binding,
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

        public SuperObservableCollection<ViewDataEntryCompletionist> Entries { get; }
            = new SuperObservableCollection<ViewDataEntryCompletionist>();

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

        protected internal NetworkSyncList<DataEntryCompletionist> Source
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
                var list = new List<ViewDataEntryCompletionist>();
                var entriesRewardAvailable = new HashSet<IProtoEntity>();
                var entriesRewardClaimed = new HashSet<IProtoEntity>();

                foreach (var sourceEntry in this.source.OrderBy(e => e.Prototype.Name,
                                                                StringComparer.InvariantCultureIgnoreCase))
                {
                    var prototype = sourceEntry.Prototype;
                    if (sourceEntry.IsRewardClaimed)
                    {
                        entriesRewardClaimed.Add(prototype);
                        continue;
                    }

                    var viewModel = this.binding[prototype];
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
            NetworkSyncList<DataEntryCompletionist> source,
            int index,
            DataEntryCompletionist value)
        {
            var prototype = value.Prototype;
            this.RemoveEntry(prototype);

            var viewModel = this.binding[prototype];
            viewModel.State = value.IsRewardClaimed
                                  ? ViewModelWindowCompletionist.CompletionistEntryState.RewardClaimed
                                  : ViewModelWindowCompletionist.CompletionistEntryState.RewardAvailable;

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

            if (viewModel.State == ViewModelWindowCompletionist.CompletionistEntryState.RewardAvailable)
            {
                NotificationSystem.ClientShowNotification(
                    viewModel.Title,
                    viewModel.NotificationMessage,
                    icon: viewModel.IconTexture,
                    color: NotificationColor.Good,
                    onClick: Menu.Open<WindowCompletionist>);
            }
        }

        private void SourceListElementRemovedHandler(
            NetworkSyncList<DataEntryCompletionist> networkSyncList,
            int removedAtIndex,
            DataEntryCompletionist removedValue)
        {
            var prototype = removedValue.Prototype;
            this.RemoveEntry(prototype);
            this.RefreshProgress();
        }
    }
}