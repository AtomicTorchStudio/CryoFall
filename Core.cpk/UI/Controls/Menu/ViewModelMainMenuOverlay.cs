namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Account;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelMainMenuOverlay : BaseViewModel
    {
        private static ViewModelMainMenuOverlay instance;

        private bool isCurrentGameTabEnabled;

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

            if (Client.ExternalApi.IsExternalClient)
            {
                Client.ExternalApi.IsLinkedAccountPropertyChanged
                    += () => this.NotifyPropertyChanged(nameof(this.CommandLinkExternalAccountVisibility));
            }

            Client.MasterServer.DemoVersionInfoChanged += this.MasterServerDemoVersionInfoChangedHandler;

            MainMenuOverlay.IsHiddenChanged += this.MainMenuOverlayOnIsHiddenChanged;
        }

        public static ViewModelMainMenuOverlay Instance
            => instance ??= new ViewModelMainMenuOverlay();

        public Visibility CommandLinkExternalAccountVisibility
            => Client.ExternalApi.IsExternalClient
               && !Client.ExternalApi.IsLinkedAccount
               && !Client.MasterServer.IsDemoVersion
                   ? Visibility.Visible
                   : Visibility.Collapsed;

        public BaseCommand CommandLogout
            => new ActionCommand(() => Client.MasterServer.LogoutPlayer());

        public Visibility CommandLogoutVisibility
            => Client.ExternalApi.IsExternalClient
                   ? Visibility.Collapsed // hide logout in Steam/Epic/etc version
                   : Visibility.Visible;

        public ICommand CommandOpenAtomicTorchTermsOfService
            => new ActionCommand(() => Client.Core.OpenWebPage(@"https://atomictorch.com/Pages/Terms-of-Service"));

        public ICommand CommandOpenDaedalicPrivacyPolicy
            => new ActionCommand(() => Client.Core.OpenWebPage(@"http://privacypolicy.daedalic.com"));

        public BaseCommand CommandOpenLinkSteamAccountWindow
            => new ActionCommand(() => Client.UI.LayoutRootChildren.Add(new WindowExternalAccountLinking()));

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

        public bool IsHomeTabSelected { get; set; } = true;

        public bool IsOptionsMenuSelected { get; set; }

        public bool IsPlayMenuVisible => !Api.IsEditor;

        public TabItem SelectedTab
        {
            get => this.selectedTab;
            set
            {
                if (this.selectedTab == value)
                {
                    return;
                }

                var options = MainMenuOverlay.GetOptions();
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

        private void ExternalApiIsLinkedAccountPropertyChangedHandler()
        {
            this.NotifyPropertyChanged(nameof(this.CommandLinkExternalAccountVisibility));
        }

        private void MainMenuOverlayOnIsHiddenChanged()
        {
            // selects "Current game" tab when connected
            if (Client.CurrentGame.ConnectionState
                    is ConnectionState.Connecting
                    or ConnectionState.Connected)
            {
                this.IsCurrentGameTabSelected = true;
            }
        }

        private void MasterServerDemoVersionInfoChangedHandler()
        {
            this.NotifyPropertyChanged(nameof(this.CommandLinkExternalAccountVisibility));
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
    }
}