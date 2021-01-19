namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Steam;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelMainMenuOverlay : BaseViewModel
    {
        private static ViewModelMainMenuOverlay instance;

        private bool isCurrentGameTabEnabled;

        private bool isServersMenuSelected;

        private TabItem selectedTab;

        private ViewModelMainMenuOverlay()
        {
            if (IsDesignTime)
            {
                // ReSharper disable once CanExtractXamlLocalizableStringCSharp
                this.Username = "username";
                return;
            }

            this.RefreshUsername();
            Client.MasterServer.LoggedInStateChanged += this.RefreshUsername;

            this.RefreshOverlay();
            Client.CurrentGame.ConnectionStateChanged += this.RefreshOverlay;
            LoadingSplashScreenManager.Instance.StateChanged += this.RefreshOverlay;

            if (Client.SteamApi.IsSteamClient)
            {
                Client.SteamApi.IsLinkedAccountPropertyChanged += this.SteamApiIsLinkedAccountPropertyChangedHandler;
            }

            Client.MasterServer.DemoVersionInfoChanged += this.MasterServerDemoVersionInfoChangedHandler;
        }

        public static ViewModelMainMenuOverlay Instance
            => instance ??= new ViewModelMainMenuOverlay();

        public Visibility CommandLinkSteamAccountVisibility
            => Client.SteamApi.IsSteamClient
               && !Client.SteamApi.IsLinkedAccount
               && !Client.MasterServer.IsDemoVersion
                   ? Visibility.Visible
                   : Visibility.Collapsed;

        public BaseCommand CommandLogout
            => new ActionCommand(() => Client.MasterServer.LogoutPlayer());

        public Visibility CommandLogoutVisibility =>
            // in Steam version we must hide the logout button (as single Steam account can have only a single AtomicTorch.com account)
            Client.SteamApi.IsSteamClient
                ? Visibility.Collapsed
                : Visibility.Visible;

        public ICommand CommandOpenDaedalicPrivacyPolicy
            => new ActionCommand(() => Client.Core.OpenWebPage(@"http://privacypolicy.daedalic.com"));

        public BaseCommand CommandOpenLinkSteamAccountWindow
            => new ActionCommand(
                () =>
                {
                    var window = new WindowSteamAccountLinking();
                    Client.UI.LayoutRootChildren.Add(window);
                });

        // ReSharper disable once CanExtractXamlLocalizableStringCSharp
        public string GameVersion { get; } = "v" + Api.Shared.GameVersionNumberWithBuildNumber;

        public bool IsCurrentGameTabEnabled
        {
            get => this.isCurrentGameTabEnabled;
            set
            {
                if (this.isCurrentGameTabEnabled == value)
                {
                    return;
                }

                this.isCurrentGameTabEnabled = value;
                this.NotifyThisPropertyChanged();
            }
        }

        public bool IsCurrentGameTabSelected { get; set; }

        public bool IsExtrasMenuSelected { get; set; }

        public bool IsOptionsMenuSelected { get; set; }

        public bool IsServersMenuSelected
        {
            get => this.isServersMenuSelected;
            set
            {
                if (this.isServersMenuSelected == value)
                {
                    return;
                }

                this.isServersMenuSelected = value;
                this.NotifyThisPropertyChanged();

                ViewModelMenuServers.Instance?.ResetSortOrder();

                // ensure master server is connected
                Client.MasterServer.Connect();
            }
        }

        public bool IsServersMenuVisible => !Api.IsEditor;

        public TabItem SelectedTab
        {
            get => this.selectedTab;
            set
            {
                if (this.selectedTab == value)
                {
                    return;
                }

                var options = MainMenuOverlay.Instance.Options;
                if (options is not null)
                {
                    if (ReferenceEquals(this.selectedTab?.Content, options))
                    {
                        // switching away from menu options
                        if (!options.CheckCanHide(
                                () => this.SelectedTab = value))
                        {
                            // don't allow switching tab now
                            this.NotifyThisPropertyChanged();
                            return;
                        }
                    }
                }

                this.selectedTab = value;

                if (options is not null
                    && ReferenceEquals(this.selectedTab?.Content, options))
                {
                    options.SelectFirstTab();
                }

                this.NotifyThisPropertyChanged();
            }
        }

        public string Username { get; private set; }

        private void MasterServerDemoVersionInfoChangedHandler()
        {
            this.NotifyPropertyChanged(nameof(this.CommandLinkSteamAccountVisibility));
        }

        private void RefreshOverlay()
        {
            var gameServerConnectionState = Client.CurrentGame.ConnectionState;
            var loadingSplashScreenState = LoadingSplashScreenManager.Instance.CurrentState;

            this.IsCurrentGameTabEnabled = gameServerConnectionState == ConnectionState.Connected;

            var isOverlayHidden = gameServerConnectionState == ConnectionState.Connected
                                  && (loadingSplashScreenState == LoadingSplashScreenState.Hiding
                                      || loadingSplashScreenState == LoadingSplashScreenState.Hidden);

            MainMenuOverlayBackground.IsHidden = isOverlayHidden;
        }

        private void RefreshUsername()
        {
            this.Username = Client.MasterServer.CurrentPlayerUsername;
        }

        private void SteamApiIsLinkedAccountPropertyChangedHandler()
        {
            this.NotifyPropertyChanged(nameof(this.CommandLinkSteamAccountVisibility));
        }
    }
}