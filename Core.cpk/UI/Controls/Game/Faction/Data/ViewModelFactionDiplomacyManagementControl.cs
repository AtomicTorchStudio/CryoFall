namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Faction.Data
{
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelFactionDiplomacyManagementControl : BaseViewModel
    {
        private readonly FactionPrivateState factionPrivateState;

        public ViewModelFactionDiplomacyManagementControl()
        {
            FactionSystem.ClientCurrentFactionMemberAddedOrRemoved
                += this.ClientCurrentFactionMemberAddedOrRemovedHandler;

            this.factionPrivateState = Faction.GetPrivateState(FactionSystem.ClientCurrentFaction);
            this.factionPrivateState.AccessRightsBinding.ClientAnyModification
                += this.AccessRightsAnyModificationHandler;
        }

        public string ClanTagForAllianceRequest { get; set; }

        public string ClanTagForWarDeclaration { get; set; }

        public BaseCommand CommandDeclareWar
            => new ActionCommand(this.ExecuteCommandDeclareWar);

        public BaseCommand CommandProposeAlliance
            => new ActionCommand(this.ExecuteCommandProposeAlliance);

        public bool HasManagementAccessRight
            => FactionSystem.ClientHasAccessRight(FactionMemberAccessRights.DiplomacyManagement);

        public bool IsAlliancesEnabled => FactionConstants.SharedPvpAlliancesEnabled;

        protected override void DisposeViewModel()
        {
            FactionSystem.ClientCurrentFactionMemberAddedOrRemoved
                -= this.ClientCurrentFactionMemberAddedOrRemovedHandler;

            this.factionPrivateState.AccessRightsBinding.ClientAnyModification
                += this.AccessRightsAnyModificationHandler;

            base.DisposeViewModel();
        }

        private void AccessRightsAnyModificationHandler(
            NetworkSyncDictionary<FactionMemberRole, FactionMemberAccessRights> source)
        {
            this.Refresh();
        }

        private void ClientCurrentFactionMemberAddedOrRemovedHandler((FactionMemberEntry entry, bool isAdded) _)
        {
            this.Refresh();
        }

        private void ExecuteCommandDeclareWar()
        {
            var clanTag = this.ClanTagForWarDeclaration;
            clanTag = FactionSystem.ClientSanitizeClanTag(clanTag);
            if (!FactionSystem.ClientIsValidClanTagForRequest(clanTag))
            {
                return;
            }

            DialogWindow.ShowDialog(
                title: CoreStrings.QuestionAreYouSure,
                text: CoreStrings.Faction_Diplomacy_DeclareWar,
                okText: CoreStrings.Yes,
                okAction: () =>
                          {
                              FactionSystem.ClientOfficerSetDiplomacyStatus(
                                  clanTag,
                                  FactionSystem.FactionDiplomacyStatusChangeRequest.Enemy);
                              this.ClanTagForWarDeclaration = null;
                          },
                cancelText: CoreStrings.Button_Cancel,
                cancelAction: () => { },
                focusOnCancelButton: true);
        }

        private void ExecuteCommandProposeAlliance()
        {
            var clanTag = this.ClanTagForAllianceRequest;
            clanTag = FactionSystem.ClientSanitizeClanTag(clanTag);
            if (!FactionSystem.ClientIsValidClanTagForRequest(clanTag))
            {
                return;
            }

            FactionSystem.ClientOfficerProposeAlliance(clanTag);
            this.ClanTagForAllianceRequest = null;
        }

        private void Refresh()
        {
            this.NotifyPropertyChanged(nameof(this.HasManagementAccessRight));
        }
    }
}