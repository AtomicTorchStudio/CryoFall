namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.Creative;
    using AtomicTorch.CBND.CoreMod.Systems.Party;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core.Data;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelWorldObjectOwnersEditor : BaseViewModel
    {
        private readonly Action<List<string>> callbackServerSetOwnersList;

        private readonly Func<string, bool> ownersListFilter;

        private readonly NetworkSyncList<string> ownersSyncList;

        public ViewModelWorldObjectOwnersEditor(
            NetworkSyncList<string> ownersSyncList,
            Action<List<string>> callbackServerSetOwnersList,
            string title,
            string emptyListMessage = CoreStrings.ObjectOwnersList_Empty,
            bool canEditOwners = true,
            Func<string, bool> ownersListFilter = null)
        {
            this.Title = title;
            this.EmptyListMessage = emptyListMessage;
            this.CanEditOwners = canEditOwners;

            this.ownersSyncList = ownersSyncList;
            this.callbackServerSetOwnersList = callbackServerSetOwnersList;
            this.ownersListFilter = ownersListFilter;

            this.ownersSyncList.ClientAnyModification += this.OwnersSyncListModificationHandler;
            this.RefreshOwnersList();

            PartySystem.ClientCurrentPartyMemberAddedOrRemoved += this.CurrentPartyMemberAddedOrRemovedHandler;
            this.RefreshHasPartyMembers();
        }

        public bool CanEditOwners { get; }

        public BaseCommand CommandAddNewOwner
            => new ActionCommand(this.ExecuteCommandAddNewOwner);

        public BaseCommand CommandAddPartyMembers
            => new ActionCommand(this.ExecuteCommandAddPartyMembers);

        public ActionCommandWithParameter CommandRemoveOwner
            => new ActionCommandWithParameter(
                arg => this.ExecuteCommandRemoveOwner((string)arg));

        public string EmptyListMessage { get; }

        public bool HasPartyMembers { get; private set; }

        public string NewOwnerName { get; set; }

        public IReadOnlyList<NameEntry> Owners { get; private set; }

        public string Title { get; set; }

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();
            this.ownersSyncList.ClientAnyModification -= this.OwnersSyncListModificationHandler;
            PartySystem.ClientCurrentPartyMemberAddedOrRemoved -= this.CurrentPartyMemberAddedOrRemovedHandler;
        }

        private void CurrentPartyMemberAddedOrRemovedHandler((string name, bool isAdded) obj)
        {
            this.RefreshHasPartyMembers();
        }

        private void ExecuteCommandAddNewOwner()
        {
            var name = this.NewOwnerName?.Trim() ?? string.Empty;
            if (name.Length == 0)
            {
                return;
            }

            if (this.ownersSyncList.Contains(name, StringComparer.OrdinalIgnoreCase))
            {
                Logger.Warning("The owner is already added (there is already an owner with the entered name).");
                return;
            }

            var newList = this.ownersSyncList.ToList();
            newList.Add(name);
            this.callbackServerSetOwnersList(newList);
            this.NewOwnerName = string.Empty;
        }

        private void ExecuteCommandAddPartyMembers()
        {
            DialogWindow.ShowDialog(
                CoreStrings.QuestionAreYouSure,
                CoreStrings.ObjectOwnersList_AddPartyMembers_Dialog,
                okText: CoreStrings.Yes,
                cancelText: CoreStrings.Button_Cancel,
                okAction: () =>
                          {
                              var partyMembers = PartySystem.ClientGetCurrentPartyMembers();
                              var newList = this.ownersSyncList.ToList()
                                                .Concat(partyMembers)
                                                .Distinct()
                                                .ToList();
                              this.callbackServerSetOwnersList(newList);
                          },
                cancelAction: () => { },
                focusOnCancelButton: true);
        }

        private void ExecuteCommandRemoveOwner(string name)
        {
            var newList = this.ownersSyncList.ToList();
            if (newList.Remove(name))
            {
                this.callbackServerSetOwnersList(newList);
            }
        }

        private Visibility GetNameEntryRemoveButtonVisibility(string name)
        {
            if (!this.CanEditOwners)
            {
                return Visibility.Collapsed;
            }

            var isCurrentPlayerCharacter = ClientCurrentCharacterHelper.Character?.Name == name;
            if (isCurrentPlayerCharacter
                && !CreativeModeSystem.ClientIsInCreativeMode())
            {
                // cannot remove self
                return Visibility.Collapsed;
            }

            return Visibility.Visible;
        }

        private void OwnersSyncListModificationHandler(NetworkSyncList<string> source)
        {
            this.RefreshOwnersList();
        }

        private void RefreshHasPartyMembers()
        {
            this.HasPartyMembers = PartySystem.ClientGetCurrentPartyMembers().Count > 1;
        }

        private void RefreshOwnersList()
        {
            IEnumerable<string> entries = this.ownersSyncList;
            if (this.ownersListFilter != null)
            {
                entries = entries.Where(this.ownersListFilter);
            }

            var list = entries.ToList();
            list.Sort(StringComparer.OrdinalIgnoreCase);

            var owners = list.Select(
                                 name => new NameEntry(
                                     name,
                                     this.CommandRemoveOwner,
                                     // player cannot remove itself from the owners list
                                     removeButtonVisibility: this.GetNameEntryRemoveButtonVisibility(name)))
                             .ToList();

            this.Owners = owners;
            //Logger.Dev("Owners list:" + Environment.NewLine + this.ownersSyncList.GetJoinedString());
        }
    }
}