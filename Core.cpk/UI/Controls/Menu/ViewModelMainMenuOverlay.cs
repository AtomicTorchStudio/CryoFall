namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Options;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Steam;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelMainMenuOverlay : BaseViewModel
    {
        private readonly MenuOptions menuOptions;

        private bool isCurrentGameTabEnabled;

        private TabItem selectedTab;

        public ViewModelMainMenuOverlay(MenuOptions menuOptions)
        {
            this.menuOptions = menuOptions;
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
                Client.SteamApi.IsLinkedAccountPropertyChanged += this.SteamApi_IsLinkedAccountPropertyChanged;
            }
        }

        public Visibility CommandLinkSteamAccountVisibility
            => Api.Client.SteamApi.IsSteamClient
               && !Api.Client.SteamApi.IsLinkedAccount
                   ? Visibility.Visible
                   : Visibility.Collapsed;

        public BaseCommand CommandLogout => new ActionCommand(this.ExecuteCommandLogout);

        public Visibility CommandLogoutVisibility =>
            // in Steam version we must hide the logout button (as single Steam account can have only a single AtomicTorch.com account)
            Api.Client.SteamApi.IsSteamClient
                ? Visibility.Collapsed
                : Visibility.Visible;

        public ICommand CommandOpenDaedalicPrivacyPolicy
            => new ActionCommand(
                () => Api.Client.Core.OpenWebPage(@"http://privacypolicy.daedalic.com"));

        public BaseCommand CommandOpenLinkSteamAccountWindow
            => new ActionCommand(this.ExecuteCommandOpenLinkSteamAccountWindow);

        // ReSharper disable once CanExtractXamlLocalizableStringCSharp
        public string GameVersion { get; } = "v" + Api.Shared.GameVersionNumberWithBuildNumber;

        public bool IsCurrentGameTabEnabled
        {
            get => this.isCurrentGameTabEnabled;
            set
            {
                this.SetProperty(ref this.isCurrentGameTabEnabled, value);
                this.IsCurrentGameTabVisible = value // && !Api.IsEditor
                                                   ? Visibility.Visible
                                                   : Visibility.Collapsed;
            }
        }

        public bool IsCurrentGameTabSelected { get; set; }

        public Visibility IsCurrentGameTabVisible { get; private set; }

        public bool IsExtrasMenuSelected { get; set; }

        public bool IsOptionsMenuSelected { get; set; }

        public Visibility IsServersMenuVisible => Api.IsEditor
                                                      ? Visibility.Collapsed
                                                      : Visibility.Visible;

        public TabItem SelectedTab
        {
            get => this.selectedTab;
            set
            {
                if (this.selectedTab == value)
                {
                    return;
                }

                //Logger.WriteDev("Switching to tab: " + value);

                if (this.selectedTab?.Content == this.menuOptions)
                {
                    // switching away from menu options
                    if (!this.menuOptions.CheckCanHide(
                            () => this.SelectedTab = value))
                    {
                        // don't allow switching tab now
                        this.NotifyThisPropertyChanged();
                        return;
                    }
                }

                this.selectedTab = value;

                if (this.selectedTab?.Content == this.menuOptions)
                {
                    this.menuOptions.SelectFirstTab();
                }

                this.NotifyThisPropertyChanged();
            }
        }

        public string Username { get; private set; }

        public bool CheckCanHide(Action callbackOnHide)
        {
            return this.menuOptions.CheckCanHide(callbackOnHide);
        }

        private void ExecuteCommandLogout()
        {
            Client.MasterServer.LogoutPlayer();
        }

        private void ExecuteCommandOpenLinkSteamAccountWindow()
        {
            var window = new WindowSteamAccountLinking();
            Client.UI.LayoutRootChildren.Add(window);
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

        private void SteamApi_IsLinkedAccountPropertyChanged()
        {
            this.NotifyPropertyChanged(nameof(this.CommandLinkSteamAccountVisibility));
        }
    }
}