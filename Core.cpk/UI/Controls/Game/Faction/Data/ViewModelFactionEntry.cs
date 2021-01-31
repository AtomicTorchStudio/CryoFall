namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Faction.Data
{
    using System.Collections.Specialized;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ViewModelFactionEntry : BaseViewModel
    {
        private readonly FactionListEntry entry;

        public ViewModelFactionEntry(
            FactionListEntry entry,
            bool isLeaderboardEntry,
            bool isPreviewEntry)
        {
            this.IsLeaderboardEntry = isLeaderboardEntry;
            this.IsPreviewEntry = isPreviewEntry;
            this.entry = entry;

            FactionSystem.ClientCurrentSubmittedApplications.CollectionChanged
                += this.SubmittedApplicationsListChangedHandler;

            FactionSystem.ClientCurrentReceivedInvitations.CollectionChanged
                += this.ReceivedInvitationsListChangedHandler;

            this.RefreshActiveInvitation();
            this.RefreshActiveApplication();

            // Preload the emblem texture.
            // Otherwise it will not load until the emblem is requested by UI
            // which causes a significant delay before the emblem could be rendered.
            ClientFactionEmblemCache.GetEmblemTextureBrush(this.entry.ClanTag);
        }

        public ApplicationDataEntry ActiveApplication { get; private set; }

        public InvitationDataEntry ActiveInvitation { get; private set; }

        public bool CanJoin
            => FactionSystem.ClientCurrentFaction is null
               && this.entry.Kind == FactionKind.Public
               && this.ActiveApplication is null
               && this.ActiveInvitation is null;

        public bool CanSubmitApplication
            => FactionSystem.ClientCurrentFaction is null
               && this.entry.Kind == FactionKind.Private
               && this.entry.IsAcceptingApplications
               && this.ActiveApplication is null
               && this.ActiveInvitation is null;

        public string ClanTag => this.entry.ClanTag;

        public BaseCommand CommandJoin
            => new ActionCommand(() => FactionSystem.ClientJoin(this.ClanTag));

        public BaseCommand CommandOpenDescription
            => new ActionCommand(this.ExecuteCommandOpenDescription);

        public BaseCommand CommandOpenOptionsPopup
            => new ActionCommand(this.ExecuteCommandOpenOptionsPopup);

        public BaseCommand CommandSubmitApplication
            => new ActionCommand(() => FactionSystem.ClientApplicantSubmitApplication(this.ClanTag));

        public bool DisplayNotAcceptingApplications
            => FactionSystem.ClientCurrentFaction is null
               && this.entry.Kind == FactionKind.Private
               && !this.entry.IsAcceptingApplications
               && this.ActiveApplication is null
               && this.ActiveInvitation is null;

        public Brush Emblem
            => ClientFactionEmblemCache.GetEmblemTextureBrush(this.entry.ClanTag);

        public string FactionKindDescription
            => this.entry.Kind.GetAttribute<DescriptionTooltipAttribute>()
                   .TooltipMessage;

        public string FactionKindTitle
            => this.entry.Kind.GetDescription();

        public byte FactionLevel => this.entry.FactionLevel;

        public bool IsDiplomacyFeatureAvailable
            => FactionSystem.SharedIsDiplomacyFeatureAvailable;

        public bool IsLeaderboardEntry { get; }

        public bool IsPreviewEntry { get; }

        public bool IsPublicFaction => this.entry.Kind == FactionKind.Public;

        public ushort LeaderboardRank => this.entry.LeaderboardRank;

        public string LeaderName => this.entry.LeaderName;

        public ushort MembersNumberCurrent => this.entry.MembersNumberCurrent;

        public ushort MembersNumberMax
            => FactionConstants.GetFactionMembersMax(this.entry.Kind);

        public string MembersNumberText
            => string.Format(CoreStrings.Faction_MembersNumber_Format,
                             this.MembersNumberCurrent,
                             this.MembersNumberMax);

        public string PublicDescription => this.entry.PublicDescription;

        public ulong TotalScore => this.entry.TotalScore;

        public string TotalScoreString => this.entry.TotalScore.ToString("#,##0");

        protected override void DisposeViewModel()
        {
            FactionSystem.ClientCurrentSubmittedApplications.CollectionChanged
                -= this.SubmittedApplicationsListChangedHandler;

            FactionSystem.ClientCurrentReceivedInvitations.CollectionChanged
                -= this.ReceivedInvitationsListChangedHandler;

            base.DisposeViewModel();
        }

        private void ExecuteCommandOpenDescription()
        {
            FactionDetailsControl.Show(this.ClanTag);
        }

        private void ExecuteCommandOpenOptionsPopup()
        {
            ClientFactionContextMenu.Show(
                this.ClanTag,
                addShowFactionInformationMenuEntry: this.IsPreviewEntry);
        }

        private FactionSystem.ClientApplicationEntry? FindApplicationEntry()
        {
            var clanTag = this.ClanTag;
            foreach (var application in FactionSystem.ClientCurrentSubmittedApplications)
            {
                if (application.ClanTag == clanTag)
                {
                    return application;
                }
            }

            return null;
        }

        private FactionSystem.ClientInvitationEntry? FindInvitationEntry()
        {
            var clanTag = this.ClanTag;
            foreach (var invitation in FactionSystem.ClientCurrentReceivedInvitations)
            {
                if (invitation.ClanTag == clanTag)
                {
                    return invitation;
                }
            }

            return null;
        }

        private void ReceivedInvitationsListChangedHandler(
            object sender,
            NotifyCollectionChangedEventArgs e)
        {
            this.RefreshActiveInvitation();
        }

        private void RefreshActiveApplication()
        {
            var application = this.FindApplicationEntry();
            this.ActiveApplication = application.HasValue
                                         ? new ApplicationDataEntry(application.Value)
                                         : null;
            this.NotifyPropertyChanged(nameof(this.CanJoin));
            this.NotifyPropertyChanged(nameof(this.CanSubmitApplication));
        }

        private void RefreshActiveInvitation()
        {
            var invitation = this.FindInvitationEntry();
            this.ActiveInvitation = invitation.HasValue
                                        ? new InvitationDataEntry(invitation.Value)
                                        : null;
            this.NotifyPropertyChanged(nameof(this.CanJoin));
            this.NotifyPropertyChanged(nameof(this.CanSubmitApplication));
        }

        private void SubmittedApplicationsListChangedHandler(
            object sender,
            NotifyCollectionChangedEventArgs e)
        {
            this.RefreshActiveApplication();
        }

        public class ApplicationDataEntry
        {
            private readonly FactionSystem.ClientApplicationEntry entry;

            public ApplicationDataEntry(FactionSystem.ClientApplicationEntry entry)
            {
                this.entry = entry;
            }

            public BaseCommand CommandCancelApplication
                => new ActionCommand(() => FactionSystem.ClientApplicantCancelApplication(this.entry.ClanTag));

            public double ExpirationDate => this.entry.ExpirationDate;
        }

        public class InvitationDataEntry
        {
            private readonly FactionSystem.ClientInvitationEntry entry;

            public InvitationDataEntry(FactionSystem.ClientInvitationEntry entry)
            {
                this.entry = entry;
            }

            public BaseCommand CommandAcceptInvitation
                => new ActionCommand(() => FactionSystem.ClientInvitationAccept(this.entry.ClanTag));

            public BaseCommand CommandDeclineInvitation
                => new ActionCommand(() => FactionSystem.ClientInvitationDecline(this.entry.ClanTag));

            public double ExpirationDate => this.entry.ExpirationDate;

            public string InviterName => this.entry.InviterName;
        }
    }
}