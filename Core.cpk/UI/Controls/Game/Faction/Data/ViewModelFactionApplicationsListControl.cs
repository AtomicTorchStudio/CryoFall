namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Faction.Data
{
    using System;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ViewModelFactionApplicationsListControl : BaseViewModel
    {
        public ViewModelFactionApplicationsListControl()
        {
            this.Reset();

            // uncomment to test the long list
            /*for (var i = 0; i < FactionConstants.FactionApplicationListEntriesLimit; i++)
            {
                this.Entries.Add(
                    new FactionApplicationOfficerViewEntry(
                        new FactionSystem.ClientApplicationEntry(
                            clanTag: null,
                            applicantName: "user" + i.ToString("000"),
                            expirationDate: double.MaxValue,
                            learningPointsAccumulatedTotal: (ushort)RandomHelper.Next(0, ushort.MaxValue))));
            }*/

            this.SortMembersList();

            FactionSystem.ClientCurrentFactionReceivedApplications.CollectionChanged
                += this.ReceivedApplicationsCollectionChangedHandler;
        }

        public ObservableCollection<FactionApplicationOfficerViewEntry> Entries { get; }
            = new();

        protected override void DisposeViewModel()
        {
            FactionSystem.ClientCurrentFactionReceivedApplications.CollectionChanged
                -= this.ReceivedApplicationsCollectionChangedHandler;
            base.DisposeViewModel();
        }

        private void ReceivedApplicationsCollectionChangedHandler(
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
                        if (entry.ApplicantName == ((FactionSystem.ClientApplicationEntry)oldItem).ApplicantName)
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
                    this.Entries.Add(new FactionApplicationOfficerViewEntry(
                                         (FactionSystem.ClientApplicationEntry)newItem));
                }

                this.SortMembersList();
            }
        }

        private void Reset()
        {
            this.Entries.Clear();
            foreach (var entry in FactionSystem.ClientCurrentFactionReceivedApplications)
            {
                this.Entries.Add(new FactionApplicationOfficerViewEntry(entry));
            }

            this.SortMembersList();
        }

        private void SortMembersList()
        {
            this.Entries.ApplySortOrder(SortComparer);

            static int SortComparer(FactionApplicationOfficerViewEntry a, FactionApplicationOfficerViewEntry b)
                => StringComparer.Ordinal.Compare(a.ApplicantName, b.ApplicantName);
        }
    }
}