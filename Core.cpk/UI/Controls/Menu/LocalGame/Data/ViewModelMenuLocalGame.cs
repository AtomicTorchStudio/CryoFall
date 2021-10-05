namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.LocalGame.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelMenuLocalGame : BaseViewModel
    {
        private bool isSelected;

        private bool isTabNewWorldSelected;

        private DataEntryLocalServerSaveGame? selectedSaveGame;

        public ViewModelMenuLocalGame()
        {
            Client.LocalServer.StatusChanged += this.LocalServerStatusChangedHandler;
            Client.CurrentGame.ConnectionStateChanged += this.CurrentGameConnectionStateChangedHandler;
        }

        public BaseCommand CommandBrowseSavesFolder
            => new ActionCommand(() => Api.Client.LocalServer.BrowseSavesFolder());

        public BaseCommand CommandDisconnect
            => new ActionCommand(() => Api.Client.CurrentGame.Disconnect());

        public bool IsLocalServerRunning { get; private set; }

        public bool IsLocalServerRunningOrStopping
            => this.IsLocalServerRunning
               || this.IsLocalServerStopping;

        public bool IsLocalServerStopping { get; private set; }

        public bool IsTabNewWorldSelected
        {
            get => this.isTabNewWorldSelected;
            set
            {
                if (this.isTabNewWorldSelected == value)
                {
                    return;
                }

                this.isTabNewWorldSelected = value;
                if (this.isTabNewWorldSelected)
                {
                    this.SelectedSaveGame = null;
                }

                this.NotifyThisPropertyChanged();
            }
        }

        public IReadOnlyList<DataEntryLocalServerSaveGame> SavedGames { get; private set; }
            = Array.Empty<DataEntryLocalServerSaveGame>();

        public DataEntryLocalServerSaveGame? SelectedSaveGame
        {
            get => this.selectedSaveGame;
            set
            {
                if (this.selectedSaveGame.Equals(value))
                {
                    return;
                }

                this.selectedSaveGame = value;
                this.NotifyThisPropertyChanged();
            }
        }

        public void Refresh(bool isSelected)
        {
            var isLocalServerRunning = Client.LocalServer.Status
                                           is LocalServerStatus.Running
                                           or LocalServerStatus.Loading;
            var isLocalServerDisconnected = Client.CurrentGame.ConnectionState is ConnectionState.Disconnected
                                            || (Client.CurrentGame.ConnectionState is ConnectionState.Connected
                                                && Client.CurrentGame.ServerInfo is not null
                                                && !Client.CurrentGame.ServerInfo.ServerAddress.IsLocalServer);

            this.IsLocalServerStopping = isLocalServerRunning && isLocalServerDisconnected;
            this.IsLocalServerRunning = isLocalServerRunning && !isLocalServerDisconnected;
            this.NotifyPropertyChanged(nameof(this.IsLocalServerRunningOrStopping));

            this.isSelected = isSelected;
            this.SelectedSaveGame = null;
            if (!isSelected
                || this.IsLocalServerRunning
                || this.IsLocalServerStopping)
            {
                this.SavedGames = Array.Empty<DataEntryLocalServerSaveGame>();
                return;
            }

            this.SavedGames = Api.Client.LocalServer.SavedGames
                                 .Select(e => new DataEntryLocalServerSaveGame(e))
                                 .OrderByDescending(e => e.Date)
                                 .ToList();
        }

        protected override void DisposeViewModel()
        {
            Client.LocalServer.StatusChanged -= this.LocalServerStatusChangedHandler;
            Client.CurrentGame.ConnectionStateChanged -= this.CurrentGameConnectionStateChangedHandler;
            base.DisposeViewModel();
        }

        private void CurrentGameConnectionStateChangedHandler()
        {
            this.Refresh(isSelected: this.isSelected);
        }

        private void LocalServerStatusChangedHandler()
        {
            this.Refresh(isSelected: this.isSelected);
        }
    }
}