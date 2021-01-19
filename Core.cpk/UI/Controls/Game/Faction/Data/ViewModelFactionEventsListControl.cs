namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Faction.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core.Data;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ViewModelFactionEventsListControl : BaseViewModel
    {
        private readonly NetworkSyncList<BaseFactionEventLogEntry> recentEventsLog;

        private string filterByMemberName;

        private EventKindEntry filterSelectedEventKind;

        public ViewModelFactionEventsListControl()
        {
            var factionPrivateState = Faction.GetPrivateState(FactionSystem.ClientCurrentFaction);
            this.recentEventsLog = factionPrivateState.RecentEventsLog;
            this.recentEventsLog.ClientElementRemoved += this.RecentEventElementRemovedHandler;
            this.recentEventsLog.ClientElementInserted += this.RecentEventElementInsertedHandler;

            var list = Api.Shared.FindScriptingTypes<BaseFactionEventLogEntry>()
                          .Where(t => !t.Type.IsAbstract)
                          .Select(t => new EventKindEntry(t.Type))
                          .OrderBy(t => t.Title)
                          .ToList();
            list.Insert(0, default);
            this.EventKinds = list;

            this.RefreshList();
        }

        public SuperObservableCollection<BaseFactionEventLogEntry> Entries { get; }
            = new();

        public IReadOnlyList<EventKindEntry> EventKinds { get; }

        public string FilterByMemberName
        {
            get => this.filterByMemberName;
            set
            {
                if (this.filterByMemberName == value)
                {
                    return;
                }

                this.filterByMemberName = value;
                this.NotifyThisPropertyChanged();

                this.RefreshList();
            }
        }

        public EventKindEntry FilterSelectedEventKind
        {
            get => this.filterSelectedEventKind;
            set
            {
                if (this.filterSelectedEventKind.Equals(value))
                {
                    return;
                }

                this.filterSelectedEventKind = value;
                this.NotifyThisPropertyChanged();

                this.RefreshList();
            }
        }

        protected override void DisposeViewModel()
        {
            this.recentEventsLog.ClientElementRemoved -= this.RecentEventElementRemovedHandler;
            this.recentEventsLog.ClientElementInserted -= this.RecentEventElementInsertedHandler;
            base.DisposeViewModel();
        }

        private void RecentEventElementInsertedHandler(
            NetworkSyncList<BaseFactionEventLogEntry> source,
            int index,
            BaseFactionEventLogEntry entry)
        {
            var entryIndex = this.Entries.Count - index;
            entryIndex = Math.Max(0, entryIndex);

            if (this.Entries.Count >= entryIndex)
            {
                this.Entries.Insert(entryIndex, entry);
            }
            else
            {
                this.Entries.Add(entry);
            }
        }

        private void RecentEventElementRemovedHandler(
            NetworkSyncList<BaseFactionEventLogEntry> source,
            int index,
            BaseFactionEventLogEntry removedEntry)
        {
            this.Entries.RemoveAt(this.Entries.Count - index - 1);
        }

        private void RefreshList()
        {
            var entries = this.recentEventsLog.Reverse();

            var filterByType = this.FilterSelectedEventKind.EntryType;
            if (filterByType is not null)
            {
                entries = entries.Where(e => ReferenceEquals(filterByType, e.GetType()));
            }

            if (!string.IsNullOrEmpty(this.filterByMemberName))
            {
                entries = entries.Where(e => e.ByMemberName is not null
                                             && (e.ByMemberName.IndexOf(this.filterByMemberName,
                                                                        StringComparison.OrdinalIgnoreCase)
                                                 >= 0));
            }

            this.Entries.ClearAndAddRange(entries);
        }

        public readonly struct EventKindEntry
            : IEquatable<EventKindEntry>
        {
            public EventKindEntry(Type entryType)
            {
                this.EntryType = entryType;
            }

            public Type EntryType { get; }

            public bool HasIcon => this.EntryType is not null;

            public Brush Icon
                => this.HasIcon
                       ? Api.Client.UI.GetTextureBrush(
                           BaseFactionEventLogEntry.GetEventTextureResource(this.EntryType))
                       : null;

            public string Title
            {
                get
                {
                    if (this.EntryType is null)
                    {
                        return CoreStrings.Faction_Events_Filter_ByEventKind_All;
                    }

                    return (this.EntryType.GetCustomAttributes(typeof(DescriptionAttribute), false)
                                .FirstOrDefault() as DescriptionAttribute)?.Description
                           ?? this.EntryType.Name;
                }
            }

            public bool Equals(EventKindEntry other)
            {
                return this.EntryType == other.EntryType;
            }

            public override bool Equals(object obj)
            {
                return obj is EventKindEntry other && this.Equals(other);
            }

            public override int GetHashCode()
            {
                return (this.EntryType != null ? this.EntryType.GetHashCode() : 0);
            }
        }
    }
}