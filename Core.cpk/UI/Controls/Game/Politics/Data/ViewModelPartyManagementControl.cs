namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Politics.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.Party;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelPartyManagementControl : BaseViewModel
    {
        public const string ClanTag_NotSelected = "not selected";

        public const string DialogMessageLeavePartyConfirmation
            = "Are you sure you want to leave the party?";

        public const string DialogMessageRemovePartyMemberConfirmationFormat
            = "Are you sure you want to remove [b]{0}[/b] from the party?";

        private Party.PartyPublicState partyPublicState;

        private IStateSubscriptionOwner partyPublicStateSubscriptionStorage;

        public ViewModelPartyManagementControl()
        {
            if (IsDesignTime)
            {
                return;
            }

            PartySystem.ClientCurrentPartyChanged += this.PartyChangedHandler;
            PartySystem.ClientCurrentPartyMemberAddedOrRemoved += this.ClientCurrentPartyMemberAddedOrRemovedHandler;
            PartySystem.ClientPartyMembersMaxChanged += () =>
                                                        {
                                                            this.NotifyPropertyChanged(nameof(this.MaxPartySize));
                                                            this.Refresh();
                                                        };
            this.PartyChangedHandler();
        }

        public bool CanInvite => this.Members.Count < this.MaxPartySize;

        public string ClanTag
        {
            get
            {
                var clanTag = this.partyPublicState?.ClanTag;
                if (string.IsNullOrEmpty(clanTag))
                {
                    return ClanTag_NotSelected;
                }

                return clanTag;
            }
        }

        public BaseCommand CommandCreateParty
            => new ActionCommand(this.ExecuteCommandCreateParty);

        public BaseCommand CommandEditClanTag
            => new ActionCommand(this.ExecuteCommandEditClanTag);

        public BaseCommand CommandInvite
            => new ActionCommand(this.ExecuteCommandInvite);

        public BaseCommand CommandLeaveParty
            => new ActionCommand(this.ExecuteCommandLeaveParty);

        public ActionCommandWithParameter CommandRemoveMember
            => new ActionCommandWithParameter(
                arg => this.ExecuteCommandRemoveMember((string)arg));

        public bool HasParty => PartySystem.ClientCurrentParty is not null;

        public string InviteeName { get; set; }

        public bool IsPartyLeader { get; private set; }

        public int MaxPartySize => PartySystem.ClientPartyMembersMax;

        public IReadOnlyList<ViewModelPartyMember> Members { get; private set; }

        protected override void DisposeViewModel()
        {
            this.partyPublicStateSubscriptionStorage?.Dispose();
            this.partyPublicStateSubscriptionStorage = null;

            PartySystem.ClientCurrentPartyChanged -= this.PartyChangedHandler;
            PartySystem.ClientCurrentPartyMemberAddedOrRemoved -= this.ClientCurrentPartyMemberAddedOrRemovedHandler;

            base.DisposeViewModel();
        }

        private void ClientCurrentPartyMemberAddedOrRemovedHandler((string name, bool isAdded) obj)
        {
            this.Refresh();
        }

        private void ExecuteCommandCreateParty()
        {
            PartySystem.ClientCreateParty();
        }

        private void ExecuteCommandEditClanTag()
        {
            WindowEditClanTag window = null;
            window = new WindowEditClanTag()
            {
                ClanTag = this.partyPublicState?.ClanTag ?? string.Empty,
                OkAction = async clanTag =>
                           {
                               Logger.Important($"Clan tag selected: {clanTag}");
                               var result = await PartySystem.ClientSetClanTag(clanTag);
                               if (result)
                               {
                                   window.CloseWindow(DialogResult.Cancel);
                                   return;
                               }

                               DialogWindow.ShowDialog(
                                   title: null,
                                   text: CoreStrings.ClanTag_Exists,
                                   closeByEscapeKey: true);
                           }
            };

            Client.UI.LayoutRootChildren.Add(window);
        }

        private void ExecuteCommandInvite()
        {
            var name = this.InviteeName?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            PartySystem.ClientInviteMember(name);
            this.InviteeName = string.Empty;
        }

        private void ExecuteCommandLeaveParty()
        {
            DialogWindow.ShowDialog(
                title: CoreStrings.QuestionAreYouSure,
                text: DialogMessageLeavePartyConfirmation,
                okText: CoreStrings.Yes,
                okAction: PartySystem.ClientLeaveParty,
                cancelText: CoreStrings.Button_Cancel,
                cancelAction: () => { });
        }

        private void ExecuteCommandRemoveMember(string memberName)
        {
            if (memberName == ClientCurrentCharacterHelper.Character.Name)
            {
                this.ExecuteCommandLeaveParty();
                return;
            }

            DialogWindow.ShowDialog(
                title: CoreStrings.QuestionAreYouSure,
                text: string.Format(DialogMessageRemovePartyMemberConfirmationFormat, memberName),
                okText: CoreStrings.Yes,
                okAction: () => PartySystem.ClientRemovePartyMember(memberName),
                cancelText: CoreStrings.Button_Cancel,
                cancelAction: () => { });
        }

        private void PartyChangedHandler()
        {
            this.partyPublicStateSubscriptionStorage?.Dispose();
            this.partyPublicStateSubscriptionStorage = null;

            var party = PartySystem.ClientCurrentParty;

            this.partyPublicState = party is not null
                                        ? Party.GetPublicState(party)
                                        : null;
            if (this.partyPublicState is not null)
            {
                this.partyPublicStateSubscriptionStorage = new StateSubscriptionStorage();
                this.partyPublicState.ClientSubscribe(_ => _.ClanTag,
                                                      () => this.NotifyPropertyChanged(nameof(this.ClanTag)),
                                                      subscriptionOwner: this.partyPublicStateSubscriptionStorage);
            }

            this.Refresh();
            this.NotifyPropertyChanged(nameof(this.ClanTag));
        }

        private void Refresh()
        {
            var oldMembers = this.Members;

            try
            {
                var list = PartySystem.ClientGetCurrentPartyMembers();
                if (list.Count == 0)
                {
                    this.Members = new ViewModelPartyMember[0];
                    this.IsPartyLeader = false;
                    return;
                }

                var currentPlayerName = ClientCurrentCharacterHelper.Character.Name;
                this.IsPartyLeader = list[0] == currentPlayerName;
                var canRemoveMember = this.IsPartyLeader;
                var removeButtonVisibility = canRemoveMember
                                                 ? Visibility.Visible
                                                 : Visibility.Collapsed;

                // uncomment to test the long list
                //for (var i = 0; i < 3; i++)
                //{
                //    list = list.Concat(list).ToList();
                //}

                this.Members = list.Select(
                                       name => new ViewModelPartyMember(
                                           name,
                                           this.CommandRemoveMember,
                                           removeButtonVisibility))
                                   .ToList();
            }
            finally
            {
                this.DisposeCollection(oldMembers);

                this.NotifyPropertyChanged(nameof(this.HasParty));
                this.NotifyPropertyChanged(nameof(this.CanInvite));
            }
        }
    }
}