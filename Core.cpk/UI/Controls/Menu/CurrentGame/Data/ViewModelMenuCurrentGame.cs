namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.CurrentGame.Data
{
    using System;
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Systems.ServerOperator;
    using AtomicTorch.CBND.CoreMod.Systems.ServerWelcomeMessage;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.CBND.GameApi.ServicesClient.Servers;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ViewModelMenuCurrentGame : BaseViewModel
    {
        private readonly ICurrentGameService game = Client.CurrentGame;

        private ConnectionState? connectionState;

        private string iconHash;

        private int lastIconLoadRequestId;

        private DateTime? wipedDate;

        public ViewModelMenuCurrentGame()
        {
            if (IsDesignTime)
            {
                return;
            }

            ServerOperatorSystem.ClientIsOperatorChanged += this.IsServerOperatorChangedHandler;

            if (Api.IsEditor)
            {
                this.VisibilityConnected = this.VisibilityNotConnected = Visibility.Collapsed;
                this.VisibilityEditorMode = Visibility.Visible;
                return;
            }

            this.VisibilityEditorMode = Visibility.Collapsed;

            this.game.PingAverageChanged += this.PingAverageChangedHandler;
            this.game.PingGameChanged += this.PingGameChangedHandler;
            this.game.ConnectionStateChanged += this.ConnectionStateChangedHandler;
            this.game.ServerInfoChanged += this.ServerInfoChangedHandler;

            this.UpdateConnectionState();
            this.UpdateServerInfo();

            this.RefreshWipedDateText();
        }

        public bool CanEditWelcomeMessage => ServerOperatorSystem.ClientIsOperator();

        public BaseCommand CommandDisconnect
            => new ActionCommand(this.ExecuteCommandDisconnect);

        public BaseCommand CommandEditWelcomeMessage
            => new ActionCommand(WelcomeMessageSystem.ClientEditWelcomeMessage);

        public BaseCommand CommandShowWelcomeMessage
            => new ActionCommand(WelcomeMessageSystem.ClientShowWelcomeMessage);

        public ConnectionState ConnectionState
        {
            get => this.connectionState ?? ConnectionState.Disconnected;
            private set
            {
                if (this.connectionState == value)
                {
                    return;
                }

                this.connectionState = value;
                this.ConnectionStateText = value.ToString();

                var isConnected = value == ConnectionState.Connected
                                  || value == ConnectionState.Connecting;
                this.VisibilityConnected = isConnected ? Visibility.Visible : Visibility.Collapsed;
                this.VisibilityNotConnected = !isConnected ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public string ConnectionStateText { get; private set; } = "connection state text";

        public Brush Icon { get; private set; }

        public string IconHash
        {
            get => this.iconHash;
            private set
            {
                if (this.iconHash == value)
                {
                    return;
                }

                this.iconHash = value;
                this.Icon = null;

                this.ReloadIcon();
            }
        }

        public ushort PingAverageMilliseconds
            => (ushort)Math.Round(
                this.game.GetPingAverageSeconds(yesIKnowIShouldUsePingGameInstead: true) * 1000,
                MidpointRounding.AwayFromZero);

        public ushort PingGameMilliseconds
            => (ushort)Math.Round(
                this.game.PingGameSeconds * 1000,
                MidpointRounding.AwayFromZero);

        public ServerAddress ServerAddress { get; private set; }

        // ReSharper disable once CanExtractXamlLocalizableStringCSharp
        public string ServerDescription { get; private set; } = "Server description text";

        public string ServerName { get; private set; } = "Server name text";

        public Visibility VisibilityConnected { get; private set; }

        public Visibility VisibilityEditorMode { get; private set; }

        public Visibility VisibilityNotConnected { get; private set; }

        public DateTime? WipedDate
        {
            get => this.wipedDate;
            set
            {
                if (this.wipedDate == value)
                {
                    return;
                }

                this.wipedDate = value;
                this.NotifyThisPropertyChanged();

                this.NotifyPropertyChanged(nameof(this.WipedDateText));
            }
        }

        public string WipedDateText
            => ViewModelServerInfo.FormatWipedDate(this.wipedDate);

        public BaseCommand CommandCopyPublicGuidToClipboard
            => new ActionCommand(() => Client.Core.CopyToClipboard(this.ServerAddress.PublicGuid.ToString()));

        public async void ReloadIcon()
        {
            if (IsDesignTime)
            {
                return;
            }

            var iconLoadRequestId = ++this.lastIconLoadRequestId;

            if (string.IsNullOrEmpty(this.iconHash))
            {
                this.Icon = null;
                return;
            }

            var loadedIconImageBrush =
                await Client.MasterServer.ServersProvider.LoadServerIconBrush(this.iconHash);

            if (this.lastIconLoadRequestId == iconLoadRequestId)
            {
                this.Icon = loadedIconImageBrush;
            }
        }

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();

            ServerOperatorSystem.ClientIsOperatorChanged -= this.IsServerOperatorChangedHandler;

            if (this.game == null)
            {
                return;
            }

            this.game.PingAverageChanged -= this.PingAverageChangedHandler;
            this.game.PingGameChanged -= this.PingGameChangedHandler;
            this.game.ConnectionStateChanged -= this.ConnectionStateChangedHandler;
            this.game.ServerInfoChanged -= this.ServerInfoChangedHandler;
        }

        private void ConnectionStateChangedHandler()
        {
            this.UpdateConnectionState();
        }

        private void ExecuteCommandDisconnect()
        {
            this.game.Disconnect();
        }

        private void IsServerOperatorChangedHandler()
        {
            this.NotifyPropertyChanged(nameof(this.CanEditWelcomeMessage));
        }

        private void PingAverageChangedHandler()
        {
            this.NotifyPropertyChanged(nameof(this.PingAverageMilliseconds));
        }

        private void PingGameChangedHandler()
        {
            this.NotifyPropertyChanged(nameof(this.PingGameMilliseconds));
        }

        private void RefreshWipedDateText()
        {
            if (this.IsDisposed)
            {
                return;
            }

            this.NotifyPropertyChanged(nameof(this.WipedDateText));

            // refresh every 10 minutes
            ClientTimersSystem.AddAction(delaySeconds: 10 * 60,
                                         this.RefreshWipedDateText);
        }

        private void ServerInfoChangedHandler()
        {
            this.UpdateServerInfo();
        }

        private void UpdateConnectionState()
        {
            this.ConnectionState = this.game.ConnectionState;
        }

        private void UpdateServerInfo()
        {
            var serverInfo = this.game.ServerInfo;
            if (serverInfo == null)
            {
                // ReSharper disable once CanExtractXamlLocalizableStringCSharp
                this.ServerName = "<no server info>";
                return;
            }

            this.ServerName = serverInfo.ServerName;
            this.ServerAddress = serverInfo.ServerAddress;
            this.ServerDescription = serverInfo.Description;
            this.IconHash = serverInfo.IconHash;
            this.WipedDate = serverInfo.CreationDateUtc.ToLocalTime();
        }
    }
}