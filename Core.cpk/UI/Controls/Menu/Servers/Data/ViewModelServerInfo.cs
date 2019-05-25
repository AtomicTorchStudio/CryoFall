namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data
{
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.ServicesClient.Servers;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ViewModelServerInfo : BaseViewModel
    {
        public const string DialogCannotConnect_Message = "Please wait until the server info is acquired.";

        public const string DialogCannotConnect_Title = "Cannot connect";

        public static readonly Brush IconPlaceholderBrush
            = IsDesignTime ? Brushes.LightSlateGray : null;

        public byte AutoRefreshRequestId;

        public int ReferencesCount;

        private ServerAddress address;

        private BaseCommand commandRefresh;

        private string iconHash;

        private bool? isCompatible;

        private bool isInfoReceived;

        private bool isSelected;

        private int lastIconLoadRequestId;

        private ushort networkProtocolVersion;

        private ushort? ping;

        private string title = "...";

        public ViewModelServerInfo(
            ServerAddress address,
            bool isFavorite,
            ActionCommandWithParameter commandFavoriteToggle,
            ActionCommandWithParameter commandDisplayModsInfo,
            BaseCommand commandJoinServer)
            : base(isAutoDisposeFields: false)
        {
            this.IsFavorite = isFavorite;
            this.CommandFavoriteToggle = commandFavoriteToggle;
            this.CommandDisplayModsInfo = commandDisplayModsInfo;
            this.CommandJoinServer = commandJoinServer;
            this.address = address;
            this.Reset();
        }

#if !GAME

        public ViewModelServerInfo()
        {
            // design-time constructor
            // ReSharper disable once CanExtractXamlLocalizableStringCSharp
            this.address = new ServerAddress("atomictorch.com");
        }

#endif

        public ServerAddress Address => this.address;

        public BaseCommand CommandCopyPublicGuidToClipboard
            => new ActionCommand(() => Client.Core.CopyToClipboard(this.Address.PublicGuid.ToString()));

        public BaseCommand CommandDisplayModsInfo { get; }

        public BaseCommand CommandFavoriteToggle { get; }

        public BaseCommand CommandJoinServer { get; }

        public BaseCommand CommandRefresh
        {
            get => this.commandRefresh;
            set
            {
                if (this.commandRefresh == value)
                {
                    return;
                }

                this.commandRefresh = value;
                this.NotifyThisPropertyChanged();
                this.RefreshButtonVisibility = value != null ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public string Description { get; set; } = "Some description text.";

        public Brush Icon { get; private set; } = IconPlaceholderBrush;

        public string IconHash
        {
            get => this.iconHash;
            set
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

        public Visibility IncompatibleVisibility { get; private set; }

        public bool? IsCompatible
        {
            get => this.isCompatible;
            set
            {
                if (value == this.isCompatible)
                {
                    return;
                }

                this.isCompatible = value;
                this.IncompatibleVisibility = value.HasValue && !value.Value
                                                  ? Visibility.Visible
                                                  : Visibility.Collapsed;
            }
        }

        public bool IsFavorite { get; set; }

        public bool IsFeatured { get; set; }

        public bool IsInfoReceived
        {
            get => this.isInfoReceived;
            set
            {
                if (this.isInfoReceived == value)
                {
                    return;
                }

                this.isInfoReceived = value;
                this.NotifyThisPropertyChanged();
                this.JoinServerButtonVisibility = this.isInfoReceived ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public bool IsModded { get; set; }

        public bool IsNotAccessible { get; set; }

        public bool IsOfficial { get; set; }

        public bool IsPingMeasurementDone { get; set; }

        public bool IsPvE { get; set; }

        public bool IsPvP { get; set; }

        public bool IsSelected
        {
            get => this.isSelected;
            set
            {
                if (this.isSelected == value)
                {
                    return;
                }

                this.isSelected = value;
                if (!this.IsDisposed)
                {
                    this.NotifyThisPropertyChanged();
                }
            }
        }

        public Visibility JoinServerButtonVisibility { get; private set; }

        public Visibility LoadingDisplayVisibility { get; set; }

        public IReadOnlyList<ServerModInfo> ModsOnServer { get; set; }

        public ushort NetworkProtocolVersion { get; set; }

        public ushort? Ping
        {
            get => this.ping;
            set
            {
                if (this.ping == value)
                {
                    return;
                }

                this.ping = value;
                this.PingText = value?.ToString() ?? "...";
            }
        }

        public string PingText { get; private set; } = "100";

        public ushort PlayersOnlineCount { get; set; }

        public string PlayersText { get; set; } = "128/256";

        public Visibility RefreshButtonVisibility { get; private set; }

        public string Title
        {
            get => this.title;
            set
            {
                if (this.title == value)
                {
                    return;
                }

                this.title = value;
                this.NotifyThisPropertyChanged();
            }
        }

        public AppVersion Version { get; set; }

        public void RefreshAndDisplayPleaseWaitDialog()
        {
            // try refresh
            this.CommandRefresh?.Execute(this);
            DialogWindow.ShowDialog(
                title: null, //DialogCannotConnect_Title,
                text: DialogCannotConnect_Message,
                closeByEscapeKey: true,
                zIndexOffset: 100000);
        }

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
                this.LoadingDisplayVisibility = Visibility.Collapsed;
                return;
            }

            var loadedIconImageBrush = await Client.MasterServer.ServersProvider
                                                   .LoadServerIconBrush(this.iconHash);

            if (this.lastIconLoadRequestId == iconLoadRequestId
                && !this.IsDisposed)
            {
                this.Icon = loadedIconImageBrush;
                this.LoadingDisplayVisibility = Visibility.Collapsed;
            }
        }

        public void Reset()
        {
            this.IsInfoReceived = false;
            this.networkProtocolVersion = 0;
            this.ping = null;
            //this.Title = string.Empty;
            //this.IsFeatured = false; // do not reset is featured flag!
            this.Description = CoreStrings.PleaseWait;
            this.PingText = "...";
            this.PlayersText = "...";
            this.CommandRefresh = null;
            this.LoadingDisplayVisibility = Visibility.Visible;
            this.IncompatibleVisibility = Visibility.Collapsed;
            this.JoinServerButtonVisibility = Visibility.Collapsed;
            this.IsNotAccessible = false;
            this.IsPingMeasurementDone = false;
            this.IsCompatible = null;
            this.Version = AppVersion.Zero;
            this.IconHash = null;
            this.IsPvP = false;
            this.IsPvE = false;
        }

        /// <summary>
        /// Please note: this method must be used only by the server view models provider!
        /// </summary>
        internal void UpdateAddress(ServerAddress newAddress)
        {
            this.address = newAddress;
            this.NotifyPropertyChanged(nameof(this.Address));
        }
    }
}