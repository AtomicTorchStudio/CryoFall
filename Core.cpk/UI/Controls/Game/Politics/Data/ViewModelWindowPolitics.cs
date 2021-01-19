namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Politics.Data
{
    using System.Collections.Specialized;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.Systems.FactionLeaderboardSystem;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core.Menu;

    public class ViewModelWindowPolitics : BaseViewModel
    {
        public ViewModelWindowPolitics()
        {
            this.UpdateTimers();

            FactionSystem.ClientCurrentFactionChanged += this.CurrentFactionChangedHandler;
            FactionSystem.ClientCurrentReceivedInvitations.CollectionChanged +=
                this.ReceivedInvitationsCollectionChangedHandler;

            FactionLeaderboardSystem.ClientLeaderboardUpdatedHandler
                += this.FactionLeaderboardUpdatedHandler;
        }

        public bool HasFaction => FactionSystem.ClientHasFaction;

        public bool IsLeaderboardSelected { get; set; }

        public string JoinCooldownRemainsText
        {
            get
            {
                var privateState = ClientCurrentCharacterHelper.PrivateState;
                if (privateState is null)
                {
                    return null;
                }

                var timeRemains = privateState.LastFactionLeaveTime
                                  + FactionConstants.SharedFactionJoinCooldownDuration
                                  - Client.CurrentGame.ServerFrameTimeRounded;
                if (timeRemains <= 0)
                {
                    return null;
                }

                return ClientTimeFormatHelper.FormatTimeDuration(timeRemains);
            }
        }

        public string LeaderboardNextUpdateInText
        {
            get
            {
                var timeRemains = FactionLeaderboardSystem.ClientNextLeaderboardUpdateTime
                                  - Client.CurrentGame.ServerFrameTimeRounded;
                if (timeRemains <= 0)
                {
                    return "0";
                }

                return ClientTimeFormatHelper.FormatTimeDuration(timeRemains);
            }
        }

        public int ReceivedInvitationsCount => FactionSystem.ClientCurrentReceivedInvitations.Count;

        public void Refresh()
        {
            this.NotifyPropertyChanged(nameof(this.JoinCooldownRemainsText));
            this.NotifyPropertyChanged(nameof(this.LeaderboardNextUpdateInText));
        }

        protected override void DisposeViewModel()
        {
            FactionSystem.ClientCurrentFactionChanged -= this.CurrentFactionChangedHandler;
            FactionSystem.ClientCurrentReceivedInvitations.CollectionChanged -=
                this.ReceivedInvitationsCollectionChangedHandler;

            FactionLeaderboardSystem.ClientLeaderboardUpdatedHandler
                -= this.FactionLeaderboardUpdatedHandler;

            base.DisposeViewModel();
        }

        private void CurrentFactionChangedHandler()
        {
            this.NotifyPropertyChanged(nameof(this.HasFaction));
            if (FactionSystem.ClientHasFaction)
            {
                // reset the selected tab
                this.IsLeaderboardSelected = true;
            }
        }

        private void FactionLeaderboardUpdatedHandler()
        {
            if (!this.IsLeaderboardSelected)
            {
                return;
            }

            // force refreshing the leaderboard tab
            this.IsLeaderboardSelected = false;
            this.IsLeaderboardSelected = true;
        }

        private void ReceivedInvitationsCollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.NotifyPropertyChanged(nameof(this.ReceivedInvitationsCount));
        }

        private void UpdateTimers()
        {
            if (this.IsDisposed)
            {
                return;
            }

            // schedule next update
            ClientTimersSystem.AddAction(
                delaySeconds: 1,
                this.UpdateTimers);

            if (Menu.IsOpened<WindowPolitics>())
            {
                this.Refresh();
            }
        }
    }
}