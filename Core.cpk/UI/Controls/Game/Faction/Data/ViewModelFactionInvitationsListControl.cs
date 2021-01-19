namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Faction.Data
{
    using System;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ViewModelFactionInvitationsListControl : BaseViewModel
    {
        public ViewModelFactionInvitationsListControl()
        {
            this.Reset();

            // uncomment to test the long list
            /*for (var i = 0; i < FactionConstants.FactionInvitationListEntriesLimit / 2; i++)
            {
                this.Entries.Add(new FactionInvitationOfficerViewEntry(
                                     new FactionSystem.ClientInvitationEntry(
                                         clanTag: null,
                                         inviterName: "TestAdmin",
                                         inviteeName: "user" + i.ToString("000"),
                                         expirationDate: double.MaxValue)));
            }*/

            this.SortMembersList();

            FactionSystem.ClientCurrentFactionCreatedInvitations.CollectionChanged
                += this.CreatedInvitationsCollectionHandler;
        }

        public ObservableCollection<FactionInvitationOfficerViewEntry> Entries { get; }
            = new();

        protected override void DisposeViewModel()
        {
            FactionSystem.ClientCurrentFactionCreatedInvitations.CollectionChanged
                -= this.CreatedInvitationsCollectionHandler;
            base.DisposeViewModel();
        }

        private void CreatedInvitationsCollectionHandler(
            object sender,
            NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Move)
            {
                return;
            }

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                this.Reset();
                return;
            }

            if (e.OldItems is not null)
            {
                foreach (var oldItem in e.OldItems)
                {
                    for (var index = 0; index < this.Entries.Count; index++)
                    {
                        var entry = this.Entries[index];
                        if (entry.InviteeName == ((FactionSystem.ClientInvitationEntry)oldItem).InviteeName)
                        {
                            this.Entries.RemoveAt(index);
                            index--;
                        }
                    }
                }
            }

            if (e.NewItems is not null)
            {
                foreach (var newItem in e.NewItems)
                {
                    this.Entries.Add(new FactionInvitationOfficerViewEntry(
                                         (FactionSystem.ClientInvitationEntry)newItem));
                }

                this.SortMembersList();
            }
        }

        private void Reset()
        {
            this.Entries.Clear();
            foreach (var entry in FactionSystem.ClientCurrentFactionCreatedInvitations)
            {
                this.Entries.Add(new FactionInvitationOfficerViewEntry(entry));
            }
        }

        private void SortMembersList()
        {
            this.Entries.ApplySortOrder(SortComparer);

            static int SortComparer(FactionInvitationOfficerViewEntry a, FactionInvitationOfficerViewEntry b)
                => StringComparer.Ordinal.Compare(a.InviteeName, b.InviteeName);
        }
    }
}