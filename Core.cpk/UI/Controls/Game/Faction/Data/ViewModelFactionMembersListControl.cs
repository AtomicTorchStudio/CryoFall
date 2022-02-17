namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Faction.Data
{
    using System;
    using System.Collections.ObjectModel;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.Systems.OnlinePlayers;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ViewModelFactionMembersListControl : BaseViewModel
    {
        private readonly NetworkSyncList<FactionMemberEntry> membersList;

        private readonly FactionPrivateState privateState;

        private readonly bool sortByRole;

        public ViewModelFactionMembersListControl(bool sortByRole)
        {
            this.sortByRole = sortByRole;
            this.privateState = Faction.GetPrivateState(FactionSystem.ClientCurrentFaction);
            this.privateState.OfficerRoleTitleBinding.ClientAnyModification
                += this.OfficerRoleTitleBindingAnyModificationHandler;

            this.membersList = this.privateState.Members;
            this.RebuildMembersList();

            // uncomment to test the long list
            /*for (var i = 0; i < 120; i++)
            {
                var accessRights = (i % 10) switch
                {
                    4 => FactionMemberAccessRights.Officer,
                    _ => FactionMemberAccessRights.Member
                };

                MembersList.Add(new FactionMemberViewEntry(
                                    new FactionMemberEntry("user" + i.ToString("000"), accessRights)));
            }*/

            this.SortMembersList();

            this.membersList.ClientElementInserted += this.MembersListElementInsertedHandler;
            this.membersList.ClientElementRemoved += this.MembersListElementRemovedHandler;

            OnlinePlayersSystem.ClientPlayerAddedOrRemoved
                += this.OnlinePlayersSystemOnClientPlayerAddedOrRemovedHandler;
        }

        public ObservableCollection<FactionMemberViewEntry> MembersList { get; }
            = new();

        protected override void DisposeViewModel()
        {
            this.privateState.OfficerRoleTitleBinding.ClientAnyModification
                -= this.OfficerRoleTitleBindingAnyModificationHandler;
            this.membersList.ClientElementInserted -= this.MembersListElementInsertedHandler;
            this.membersList.ClientElementRemoved -= this.MembersListElementRemovedHandler;

            OnlinePlayersSystem.ClientPlayerAddedOrRemoved
                -= this.OnlinePlayersSystemOnClientPlayerAddedOrRemovedHandler;

            base.DisposeViewModel();
        }

        private void MembersListElementInsertedHandler(
            NetworkSyncList<FactionMemberEntry> source,
            int _,
            FactionMemberEntry entry)
        {
            this.MembersList.Add(
                new FactionMemberViewEntry(entry,
                                           isOnlineStatusAvailable: true,
                                           isOnline: OnlinePlayersSystem.ClientIsOnline(entry.Name)));
            this.SortMembersList();
        }

        private void MembersListElementRemovedHandler(
            NetworkSyncList<FactionMemberEntry> source,
            int _,
            FactionMemberEntry removedValue)
        {
            var list = this.MembersList;
            for (var index = 0; index < list.Count; index++)
            {
                var viewEntry = list[index];
                if (viewEntry.Name != removedValue.Name)
                {
                    continue;
                }

                list.RemoveAt(index);
                this.SortMembersList();
                return;
            }

            Logger.Error("Entry not found: " + removedValue.Name);
        }

        private void OfficerRoleTitleBindingAnyModificationHandler(
            NetworkSyncDictionary<FactionMemberRole, FactionOfficerRoleTitle> source)
        {
            this.RebuildMembersList();
        }

        private void OnlinePlayersSystemOnClientPlayerAddedOrRemovedHandler(
            OnlinePlayersSystem.Entry entry,
            bool isOnline)
        {
            var name = entry.Name;
            var list = this.MembersList;
            for (var index = 0; index < list.Count; index++)
            {
                var viewEntry = list[index];
                if (viewEntry.Name != name)
                {
                    continue;
                }

                list.RemoveAt(index);
                list.Insert(index, viewEntry.WithOnlineStatus(isOnline));
                return;
            }
        }

        private void RebuildMembersList()
        {
            this.MembersList.Clear();
            var factionMemberEntries = this.membersList;
            foreach (var entry in factionMemberEntries)
            {
                this.MembersList.Add(
                    new FactionMemberViewEntry(entry,
                                               isOnlineStatusAvailable: true,
                                               isOnline: OnlinePlayersSystem.ClientIsOnline(entry.Name)));
            }
        }

        private void SortMembersList()
        {
            var sortComparer = this.sortByRole
                                   ? (Comparison<FactionMemberViewEntry>)SortByRoleComparer
                                   : (Comparison<FactionMemberViewEntry>)SortByNameComparer;

            this.MembersList.ApplySortOrder(sortComparer);

            static int SortByRoleComparer(FactionMemberViewEntry a, FactionMemberViewEntry b)
            {
                if (a.Role != b.Role)
                {
                    return b.Role.CompareTo(a.Role);
                }

                return StringComparer.OrdinalIgnoreCase.Compare(a.Name, b.Name);
            }

            static int SortByNameComparer(FactionMemberViewEntry a, FactionMemberViewEntry b)
                => StringComparer.OrdinalIgnoreCase.Compare(a.Name, b.Name);
        }
    }
}