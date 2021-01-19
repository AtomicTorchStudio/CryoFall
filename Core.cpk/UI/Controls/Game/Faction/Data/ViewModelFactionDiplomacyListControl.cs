namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Faction.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core.Data;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ViewModelFactionDiplomacyListControl : BaseViewModel
    {
        private readonly FactionDiplomacyStatus diplomacyStatusFilter;

        private readonly NetworkSyncDictionary<string, FactionDiplomacyStatus> source;

        public ViewModelFactionDiplomacyListControl(FactionDiplomacyStatus diplomacyStatusFilter)
        {
            this.diplomacyStatusFilter = diplomacyStatusFilter;

            this.source = Faction.GetPrivateState(FactionSystem.ClientCurrentFaction)
                                 .FactionDiplomacyStatuses;

            this.source.ClientPairSet += this.SourcePairSetHandler;
            this.source.ClientPairRemoved += this.SourcePairRemovedHandler;
            this.source.ClientDictionaryClear += this.SourceDictionaryClearHandler;

            this.RebuildList();
        }

        public SuperObservableCollection<ViewModelFactionDiplomacyStatusEntry> Entries { get; }
            = new();

        protected override void DisposeViewModel()
        {
            this.source.ClientPairSet -= this.SourcePairSetHandler;
            this.source.ClientPairRemoved -= this.SourcePairRemovedHandler;
            this.source.ClientDictionaryClear -= this.SourceDictionaryClearHandler;

            base.DisposeViewModel();
        }

        private void RebuildList()
        {
            var newEntries = new List<ViewModelFactionDiplomacyStatusEntry>();
            foreach (var entry in this.source)
            {
                if (this.diplomacyStatusFilter != entry.Value)
                {
                    continue;
                }

                var clanTag = entry.Key;
                newEntries.Add(
                    new ViewModelFactionDiplomacyStatusEntry(clanTag,
                                                             this.diplomacyStatusFilter));
            }

            // uncomment to test the long list
            /*var randomCount = RandomHelper.Next(1, 7);
            for (var i = 0; i < randomCount; i++)
            {
                var clanTag = ((char)RandomHelper.Next('A',   'Z' + 1)).ToString()
                              + ((char)RandomHelper.Next('A', 'Z' + 1))
                              + ((char)RandomHelper.Next('A', 'Z' + 1))
                              + ((char)RandomHelper.Next('A', 'Z' + 1));
                newEntries.Add(
                    new FactionDiplomacyStatusEntry(clanTag,
                                                    this.diplomacyStatusFilter,
                                                    SharedFactionEmblemProvider.Instance.GenerateRandomEmblem()));
            }*/

            this.Entries.ClearAndAddRange(newEntries);
            this.SortList();
        }

        private void SortList()
        {
            this.Entries.ApplySortOrder(SortComparer);

            static int SortComparer(ViewModelFactionDiplomacyStatusEntry a, ViewModelFactionDiplomacyStatusEntry b)
                => string.Compare(a.ClanTag, b.ClanTag, StringComparison.Ordinal);
        }

        private void SourceDictionaryClearHandler(
            NetworkSyncDictionary<string, FactionDiplomacyStatus> source)
        {
            this.RebuildList();
        }

        private void SourcePairRemovedHandler(
            NetworkSyncDictionary<string, FactionDiplomacyStatus> source,
            string clanTag)
        {
            var entry = this.Entries.FirstOrDefault(e => e.ClanTag == clanTag);
            if (entry?.ClanTag is null)
            {
                return;
            }

            this.Entries.Remove(entry);
            this.SortList();
        }

        private void SourcePairSetHandler(
            NetworkSyncDictionary<string, FactionDiplomacyStatus> source,
            string clanTag,
            FactionDiplomacyStatus status)
        {
            // remove existing entry (if exists)
            this.SourcePairRemovedHandler(source, clanTag);

            if (status != this.diplomacyStatusFilter)
            {
                return;
            }

            this.Entries.Add(new ViewModelFactionDiplomacyStatusEntry(clanTag,
                                                                      status));
            this.SortList();
        }
    }
}