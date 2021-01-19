namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Systems.ServerWelcomeMessage;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Performance.Data;
    using AtomicTorch.CBND.CoreMod.UI.Helpers;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Servers;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ViewModelServerInfo : BaseViewModel
    {
        public const string DialogCannotConnect_Message = "Please wait until the server info is acquired.";

        public const string DialogCannotConnect_Title = "Cannot connect";

        public static readonly SolidColorBrush BrushPingDefault
            = new(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));

        public static readonly SolidColorBrush BrushPingRed
            = new(Color.FromArgb(0xFF, 0xFF, 0x66, 0x66));

        public static readonly SolidColorBrush BrushPingYellow
            = new(Color.FromArgb(0xFF, 0xFF, 0xEE, 0x88));

        public static readonly Brush IconPlaceholderBrush
            = IsDesignTime ? Brushes.LightSlateGray : null;

        public byte AutoRefreshRequestId;

        public int ReferencesCount;

        private ServerAddress address;

        private BaseCommand commandRefresh;

        private DialogWindow dialogWindowPleaseWait;

        private Action dialogWindowPleaseWaitCallbackOnInfoReceivedOrCannotReach;

        private string iconHash;

        private bool? isCompatible;

        private bool isInaccessible;

        private bool isInfoReceived;

        private bool isSelected;

        private int lastIconLoadRequestId;

        private DateTime? nextScheduledWipeDate;

        private ushort? ping = ushort.MaxValue;

        private string title = "...";

        private DateTime? wipedDate;

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
                this.RefreshButtonVisibility = value is not null ? Visibility.Visible : Visibility.Collapsed;
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

        public bool IsCommunity { get; set; }

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

        public bool IsInaccessible
        {
            get => this.isInaccessible;
            set
            {
                if (this.isInaccessible == value)
                {
                    return;
                }

                this.isInaccessible = value;

                if (this.isInaccessible)
                {
                    this.OnInfoReceivedOnInaccessible();
                }
            }
        }

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

                if (this.isInfoReceived)
                {
                    this.OnInfoReceivedOnInaccessible();
                }
            }
        }

        public bool IsModded { get; set; }

        public bool IsNoClientModsAllowed { get; set; }

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

        public DateTime? NextScheduledWipeDate
        {
            get => this.nextScheduledWipeDate;
            set
            {
                if (this.nextScheduledWipeDate == value)
                {
                    return;
                }

                this.nextScheduledWipeDate = value;
                this.NotifyThisPropertyChanged();

                this.NotifyPropertyChanged(nameof(this.NextScheduledWipeDateText));
            }
        }

        public string NextScheduledWipeDateText
            => this.nextScheduledWipeDate.HasValue
                   ? string.Format(CoreStrings.ServerWipeInfoNextWipeDate_Format,
                                   WelcomeMessageSystem.FormatDate(this.nextScheduledWipeDate.Value))
                   : null;

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
                this.NotifyThisPropertyChanged();

                this.PingText = this.ping?.ToString() ?? "...";
                this.PingForegroundBrush = GetPingForegroundBrush(this.ping);
            }
        }

        public Brush PingForegroundBrush { get; private set; } = BrushPingDefault;

        public string PingText { get; private set; } = "100";

        public ushort PlayersOnlineCount { get; set; }

        public string PlayersText { get; set; } = "128/256";

        public Visibility RefreshButtonVisibility { get; private set; }

        public string TimeAlreadyConvertedToLocalTimeZoneText
            => ClientLocalTimeZoneHelper.GetTextTimeAlreadyConvertedToLocalTimeZone();

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

        public Visibility VisibilityInList { get; set; } = Visibility.Visible;

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
            => FormatWipedDate(this.wipedDate);

        public static string FormatWipedDate(DateTime? wipedDate)
        {
            if (!wipedDate.HasValue)
            {
                return "...";
            }

            // test random wipe dates
            //this.wipedDate = DateTime.Now;
            //if (RandomHelper.RollWithProbability(0.9))
            //{
            //    this.wipedDate -= TimeSpan.FromHours(RandomHelper.Next(0, 60));
            //}

            var now = DateTime.Now;
            if (now <= wipedDate.Value)
            {
                return CoreStrings.WipedDate_JustWiped;
            }

            var deltaTime = now - wipedDate.Value;

            var totalDays = deltaTime.TotalDays;
            if (totalDays > 10000)
            {
                return "—";
            }

            if (totalDays >= 1)
            {
                if (totalDays < 2)
                {
                    return CoreStrings.WipedDate_Yesterday;
                }

                return string.Format(CoreStrings.WipedDate_DaysAgo_Format, (int)totalDays);
            }

            var totalHours = deltaTime.TotalHours;
            if (totalHours >= 2)
            {
                return string.Format(CoreStrings.WipedDate_HoursAgo_Format, (int)totalHours);
            }

            return CoreStrings.WipedDate_JustWiped;
        }

        public static Brush GetPingForegroundBrush(ushort? ping)
        {
            if (!ping.HasValue
                || ping.Value <= ViewModelNetworkPerformanceStats.PingSubstantialValue)
            {
                return BrushPingDefault;
            }

            return ping.Value <= ViewModelNetworkPerformanceStats.PingSevereValue
                       ? BrushPingYellow
                       : BrushPingRed;
        }

        public void RefreshAndDisplayPleaseWaitDialog(
            Action onInfoReceivedOrCannotReach)
        {
            if (this.IsInaccessible)
            {
                this.CommandRefresh?.Execute(this);
            }

            this.dialogWindowPleaseWait?.Close(DialogResult.Cancel);
            this.dialogWindowPleaseWait = null;

            var dialogContent = new Grid();

            // prepare dialog content
            {
                var dialogLoadingDisplay = new LoadingDisplayControl()
                {
                    Width = 48,
                    Height = 48,
                    Margin = new Thickness(0, 5, 0, 5)
                };

                dialogContent.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                dialogContent.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(10) });
                dialogContent.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

                Grid.SetRow(dialogLoadingDisplay, 2);
                dialogContent.Children.Add(dialogLoadingDisplay);

                var dialogText = DialogWindow.CreateTextElement(DialogCannotConnect_Message, TextAlignment.Center);
                dialogContent.Children.Add(dialogText);
            }

            // display dialog
            var dialog = DialogWindow.ShowDialog(
                title: null,
                content: dialogContent,
                closeByEscapeKey: true,
                cancelText: CoreStrings.Button_Cancel,
                hideOkButton: true,
                hideCancelButton: false,
                zIndexOffset: 100000,
                focusOnCancelButton: false);

            this.dialogWindowPleaseWait = dialog;
            this.dialogWindowPleaseWaitCallbackOnInfoReceivedOrCannotReach = onInfoReceivedOrCannotReach;

            dialog.Closed += DialogClosedHandler;

            void DialogClosedHandler(object s, EventArgs e)
            {
                if (dialog != this.dialogWindowPleaseWait)
                {
                    return;
                }

                this.dialogWindowPleaseWait = null;
                this.dialogWindowPleaseWaitCallbackOnInfoReceivedOrCannotReach = null;
                dialog.Closed -= DialogClosedHandler;
            }
        }

        public void RefreshVisibilityInList()
        {
            // ReSharper disable once ReplaceWithSingleAssignment.True
            var isVisible = true;

            // try apply incompatible servers filter
            if (isVisible
                && !ServerViewModelsProvider.ShowIncompatibleServers
                && this.IsCompatible.HasValue
                && !this.IsCompatible.Value)
            {
                isVisible = false;
            }

            if (isVisible
                && this.IsInfoReceived)
            {
                // try apply empty servers filter
                if (isVisible
                    && !ServerViewModelsProvider.ShowEmptyServers
                    && this.PlayersOnlineCount == 0
                    && !this.IsSelected)
                {
                    isVisible = false;
                }

                // try apply PvE servers filter
                if (isVisible
                    && !ServerViewModelsProvider.ShowPvEServers
                    && this.IsPvE)
                {
                    isVisible = false;
                }

                // try apply PvP servers filter
                if (isVisible
                    && !ServerViewModelsProvider.ShowPvPServers
                    && this.IsPvP)
                {
                    isVisible = false;
                }
            }

            this.VisibilityInList = isVisible
                                        ? Visibility.Visible
                                        : Visibility.Collapsed;
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
            this.Ping = null;
            //this.Title = string.Empty;
            //this.IsFeatured = false; // do not reset is featured flag!
            this.Description = CoreStrings.PleaseWait;
            this.PlayersOnlineCount = 0;
            this.PlayersText = "...";
            this.CommandRefresh = null;
            this.LoadingDisplayVisibility = Visibility.Visible;
            this.IncompatibleVisibility = Visibility.Collapsed;
            this.JoinServerButtonVisibility = Visibility.Collapsed;
            this.IsInaccessible = false;
            this.IsPingMeasurementDone = false;
            this.IsCompatible = null;
            this.Version = AppVersion.Zero;
            this.IconHash = null;
            this.IsPvP = false;
            this.IsPvE = false;
            this.IsNoClientModsAllowed = false;
            this.WipedDate = null;
            this.NextScheduledWipeDate = null;
            this.VisibilityInList = Visibility.Visible;
            this.IsOfficial = false;
            this.IsCommunity = false;
        }

        /// <summary>
        /// Please note: this method must be used only by the server view models provider!
        /// </summary>
        internal void UpdateAddress(ServerAddress newAddress)
        {
            this.address = newAddress;
            this.NotifyPropertyChanged(nameof(this.Address));
        }

        private void OnInfoReceivedOnInaccessible()
        {
            var callback = this.dialogWindowPleaseWaitCallbackOnInfoReceivedOrCannotReach;
            if (callback is null)
            {
                return;
            }

            this.dialogWindowPleaseWaitCallbackOnInfoReceivedOrCannotReach = null;
            this.dialogWindowPleaseWait?.Close(DialogResult.Cancel);
            Logger.Info("Server info received or server address inaccessible - executing action callback: "
                        + this.address);
            Api.SafeInvoke(callback);
        }
    }
}