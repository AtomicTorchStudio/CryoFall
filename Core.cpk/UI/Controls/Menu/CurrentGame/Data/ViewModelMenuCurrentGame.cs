namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.CurrentGame.Data
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Systems.ServerOperator;
    using AtomicTorch.CBND.CoreMod.Systems.ServerWelcomeMessage;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.CBND.GameApi.ServicesClient.Servers;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

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

            Instance = this;

            ServerOperatorSystem.ClientIsOperatorChanged += this.IsServerOperatorChangedHandler;

            this.game.PingAverageChanged += this.PingAverageChangedHandler;
            this.game.PingGameChanged += this.PingGameChangedHandler;
            this.game.ConnectionStateChanged += this.ConnectionStateChangedHandler;
            this.game.ServerInfoChanged += this.ServerInfoChangedHandler;

            this.UpdateConnectionState();
            this.UpdateServerInfo();

            this.RefreshWipedDateText();
        }

        public static ViewModelMenuCurrentGame Instance { get; private set; }

        public bool CanEditServerRates
            => this.IsServerOperator
               || this.IsLocalServer;

        public BaseCommand CommandBrowseServerRates
            => new ActionCommand(this.ExecuteCommandBrowseServerRates);

        public BaseCommand CommandCopyPublicGuidToClipboard
            => new ActionCommand(() => Client.Core.CopyToClipboard(this.ServerAddress.PublicGuid.ToString()));

        public BaseCommand CommandDisconnect
            => new ActionCommand(this.ExecuteCommandDisconnect);

        public BaseCommand CommandEditDescription
            => new ActionCommand(WelcomeMessageSystem.ClientEditDescription);

        public BaseCommand CommandEditScheduledWipeDate
            => new ActionCommand(WelcomeMessageSystem.ClientEditScheduledWipeDate);

        public BaseCommand CommandEditServerRates
            => new ActionCommand(this.ExecuteCommandEditServerRates);

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

                this.IsConnected = value is ConnectionState.Connected
                                       or ConnectionState.Connecting;
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

        public bool IsConnected { get; private set; }

        public bool IsEditor => Api.IsEditor;

        public bool IsLocalServer { get; private set; }

        public bool IsServerOperator => ServerOperatorSystem.ClientIsOperator();

        public ushort PingAverageMilliseconds
            => (ushort)Math.Round(
                this.game.GetPingAverageSeconds(yesIKnowIShouldUsePingGameInstead: true) * 1000,
                MidpointRounding.AwayFromZero);

        public Brush PingGameForegroundBrush
            => ViewModelServerInfo.GetPingForegroundBrush(this.PingGameMilliseconds);

        public ushort PingGameMilliseconds
            => (ushort)Math.Round(
                this.game.PingGameSeconds * 1000,
                MidpointRounding.AwayFromZero);

        public ServerAddress ServerAddress { get; private set; }

        // ReSharper disable once CanExtractXamlLocalizableStringCSharp
        public string ServerDescription { get; set; } = "Server description text";

        public string ServerName { get; private set; } = "Server name text";

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
            => this.ServerAddress.IsLocalServer
                   ? string.Empty
                   : ViewModelServerInfo.FormatWipedDate(this.wipedDate);

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
            if (ReferenceEquals(this, Instance))
            {
                Instance = null;
            }

            base.DisposeViewModel();

            ServerOperatorSystem.ClientIsOperatorChanged -= this.IsServerOperatorChangedHandler;

            if (this.game is null)
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

        private void ExecuteCommandBrowseServerRates()
        {
            var dialogWindow = DialogWindow.ShowDialog(
                CoreStrings.MenuCurrentGame_CurrentServerRates,
                new ScrollViewer()
                {
                    MaxHeight = 380,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    Content = new CurrentServerRatesBrowserControl()
                },
                closeByEscapeKey: false);

            dialogWindow.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            dialogWindow.GameWindow.FocusOnControl = null;
            dialogWindow.GameWindow.Width = 530;
            dialogWindow.GameWindow.UpdateLayout();
        }

        private void ExecuteCommandDisconnect()
        {
            this.game.Disconnect();
        }

        private void ExecuteCommandEditServerRates()
        {
            ClientCurrentGameServerRatesEditorHelper.OpenEditorWindow();
        }

        private void IsServerOperatorChangedHandler()
        {
            this.NotifyPropertyChanged(nameof(this.IsServerOperator));
            this.NotifyPropertyChanged(nameof(this.CanEditServerRates));
        }

        private void PingAverageChangedHandler()
        {
            this.NotifyPropertyChanged(nameof(this.PingAverageMilliseconds));
        }

        private void PingGameChangedHandler()
        {
            this.NotifyPropertyChanged(nameof(this.PingGameMilliseconds));
            this.NotifyPropertyChanged(nameof(this.PingGameForegroundBrush));
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
            this.NotifyPropertyChanged(nameof(this.CanEditServerRates));
        }

        private void UpdateServerInfo()
        {
            var serverInfo = this.game.ServerInfo;
            if (serverInfo is null)
            {
                // ReSharper disable once CanExtractXamlLocalizableStringCSharp
                this.ServerName = "<no server info>";
                return;
            }

            this.ServerAddress = serverInfo.ServerAddress;
            this.IsLocalServer = this.ServerAddress.IsLocalServer;

            if (this.IsLocalServer)
            {
                this.ServerName = string.Format(CoreStrings.LocalServerSaveName_Format,
                                                Client.LocalServer.GetSaveName(this.ServerAddress.LocalServerSlotId)
                                                ?? $"Slot #{this.ServerAddress.LocalServerSlotId}");
                this.ServerDescription = string.Empty;
                this.Icon = ViewModelServerInfo.LocalServerIconImageBrush;
                this.WipedDate = null;
                return;
            }

            this.ServerName = serverInfo.ServerName;
            this.ServerDescription = serverInfo.Description;
            this.IconHash = serverInfo.IconHash;
            this.WipedDate = serverInfo.CreationDateUtc.ToLocalTime();
        }
    }
}