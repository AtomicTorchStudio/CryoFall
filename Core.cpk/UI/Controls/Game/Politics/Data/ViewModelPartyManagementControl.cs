namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Politics.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.Party;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelPartyManagementControl : BaseViewModel
    {
        public const string DialogMessageLeavePartyConfirmation
            = "Are you sure you want to leave the party?";

        public const string DialogMessageRemovePartyMemberConfirmationFormat
            = "Are you sure you want to remove [b]{0}[/b] from the party?";

        public ViewModelPartyManagementControl()
        {
            if (IsDesignTime)
            {
                return;
            }

            PartySystem.ClientCurrentPartyChanged += this.Refresh;
            PartySystem.ClientCurrentPartyMemberAddedOrRemoved += this.ClientCurrentPartyMemberAddedOrRemovedHandler;
            PartySystem.ClientPartyMembersMaxChanged += () =>
                                                        {
                                                            this.NotifyPropertyChanged(nameof(this.MaxPartySize));
                                                            this.Refresh();
                                                        };
            this.Refresh();
        }

        public bool CanInvite => this.Members.Count < this.MaxPartySize;

        public BaseCommand CommandCreateParty
            => new ActionCommand(this.ExecuteCommandCreateParty);

        public BaseCommand CommandInvite
            => new ActionCommand(this.ExecuteCommandInvite);

        public BaseCommand CommandLeaveParty
            => new ActionCommand(this.ExecuteCommandLeaveParty);

        public ActionCommandWithParameter CommandRemoveMember
            => new ActionCommandWithParameter(
                arg => this.ExecuteCommandRemoveMember((string)arg));

        public bool HasParty => PartySystem.ClientCurrentParty != null;

        public string InviteeName { get; set; }

        public int MaxPartySize => PartySystem.ClientPartyMembersMax;

        public IReadOnlyList<ViewModelPartyMember> Members { get; private set; }

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();
            PartySystem.ClientCurrentPartyChanged -= this.Refresh;
            PartySystem.ClientCurrentPartyMemberAddedOrRemoved -= this.ClientCurrentPartyMemberAddedOrRemovedHandler;
        }

        private void ClientCurrentPartyMemberAddedOrRemovedHandler((string name, bool isAdded) obj)
        {
            this.Refresh();
        }

        private void ExecuteCommandCreateParty()
        {
            PartySystem.ClientCreateParty();
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

        private void Refresh()
        {
            var oldMembers = this.Members;

            try
            {
                var list = PartySystem.ClientGetCurrentPartyMembers();
                if (list.Count == 0)
                {
                    this.Members = new ViewModelPartyMember[0];
                    return;
                }

                var currentPlayerName = ClientCurrentCharacterHelper.Character.Name;
                var canRemoveMember = list[0] == currentPlayerName;
                var removeButtonVisibility = canRemoveMember
                                                 ? Visibility.Visible
                                                 : Visibility.Collapsed;

                // uncomment to test the long list
                //list = list.Concat(list).ToList();
                //list = list.Concat(list).ToList();
                //list = list.Concat(list).ToList();

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