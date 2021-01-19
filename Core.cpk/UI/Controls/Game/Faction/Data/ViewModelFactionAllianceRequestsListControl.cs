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

    public class ViewModelFactionAllianceRequestsListControl : BaseViewModel
    {
        private readonly NetworkSyncDictionary<string, FactionAllianceRequest> source;

        public ViewModelFactionAllianceRequestsListControl(bool isIncomingRequests)
        {
            var factionPrivateState = Faction.GetPrivateState(FactionSystem.ClientCurrentFaction);
            this.source = isIncomingRequests
                              ? factionPrivateState.IncomingFactionAllianceRequests
                              : factionPrivateState.OutgoingFactionAllianceRequests;

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
                if (entry.Value.IsRejected)
                {
                    continue;
                }

                var clanTag = entry.Key;
                var viewModelEntry = new ViewModelFactionDiplomacyStatusEntry(clanTag,
                                                                              FactionDiplomacyStatus.Neutral);
                newEntries.Add(viewModelEntry);
            }

            // uncomment to test the long list
            /*var randomCount = RandomHelper.Next(1, 7);
            for (var i = 0; i < randomCount; i++)
            {
                var clanTag = ((char)RandomHelper.Next('A', 'Z' + 1)).ToString()
                              + ((char)RandomHelper.Next('A', 'Z' + 1))
                              + ((char)RandomHelper.Next('A', 'Z' + 1))
                              + ((char)RandomHelper.Next('A', 'Z' + 1));
                newEntries.Add(
                    new ViewModelFactionDiplomacyStatusEntry(clanTag,
                                                             FactionDiplomacyStatus.Neutral));
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
            NetworkSyncDictionary<string, FactionAllianceRequest> factionAllianceRequests)
        {
            this.RebuildList();
        }

        private void SourcePairRemovedHandler(
            NetworkSyncDictionary<string, FactionAllianceRequest> factionAllianceRequests,
            string clanTag)
        {
            var entry = this.Entries.FirstOrDefault(e => e.ClanTag == clanTag);
            if (entry?.ClanTag is null)
            {
                return;
            }

            this.Entries.Remove(entry);
            entry.Dispose();
            this.SortList();
        }

        private void SourcePairSetHandler(
            NetworkSyncDictionary<string, FactionAllianceRequest> factionAllianceRequests,
            string clanTag,
            FactionAllianceRequest request)
        {
            // remove existing entry (if exists)
            this.SourcePairRemovedHandler(this.source, clanTag);

            if (request.IsRejected)
            {
                return;
            }

            this.Entries.Add(new ViewModelFactionDiplomacyStatusEntry(clanTag,
                                                                      FactionDiplomacyStatus.Neutral));
            this.SortList();
        }
    }
}