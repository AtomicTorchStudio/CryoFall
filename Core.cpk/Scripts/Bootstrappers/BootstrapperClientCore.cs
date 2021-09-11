namespace AtomicTorch.CBND.CoreMod.Bootstrappers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.ClientComponents.FX;
    using AtomicTorch.CBND.CoreMod.Helpers.Client.Music;
    using AtomicTorch.CBND.CoreMod.Helpers.Client.Server;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Cursor;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Login;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.CBND.GameApi.ServicesClient.Servers;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class BootstrapperClientCore : BaseBootstrapper
    {
        public const string DialogCannotConnectToTheMasterServer_CanPlayOnLocalServer_Message =
            @"The local game is available even if you stay offline.";

        public const string DialogCannotConnectToTheMasterServer_Message =
            @"The client was unable to establish a connection to the Master Server.
              [br]Please ensure you are connected to the Internet and your firewall
              [br]or ISP aren't blocking your connection.";

        public const string DialogCannotConnectToTheMasterServer_Title =
            "Cannot connect to the Master Server";

        public const string DialogModsMissing_MessageFormat =
            @"This game server requires the following mods:
              [br]{0}
              [br][br]Please download and install these mods to be able to join.
              [br]You can find mods on the official forums: http://forums.atomictorch.com/index.php?board=24.0";

        public const string DialogModsMissing_Title = "Cannot connect—mods missing";

        private static WeakReference cannotConnectToMasterServerDialog;

        public static void DisconnectMasterServerIfNotNecessary()
        {
            if (Client.CurrentGame.ConnectionState == ConnectionState.Connected
                && Client.MasterServer.MasterServerConnectionState != ConnectionState.Disconnected
                && MainMenuOverlay.IsHidden)
            {
                Logger.Important(
                    "Disconnecting from MasterServer as it's not necessary now (client is connected to the game server and main menu overlay is hidden)");

                // ensure the master server is disconnected as it's not necessary now
                Client.MasterServer.Disconnect();
            }
        }

        public override void ClientInitialize()
        {
            if (!Api.IsEditor
                && !Api.Shared.IsRequiresLanguageSelection)
            {
                // force master server connection in non-game mode (in non-Editor only)
                Client.MasterServer.Connect();
            }

            Client.Core.SetLoadingSplashScreenManager(LoadingSplashScreenManager.Instance);

            Client.CurrentGame.ConnectionStateChanged += CurrentGameServerConnectionChangedHandler;
            Client.CurrentGame.ConnectingFailed += GameServerLoginFailedDialogHelper.ShowDialog;
            Client.CurrentGame.ConnectingFailedModsMissing += GameServerConnectingFailedModsMissingHandler;
            RefreshCurrentGameServerConnectionState();

            Client.Core.IsCompilingChanged += RefreshCompilingState;
            Client.Core.IsCompilationFailedChanged += RefreshCompilationFailedState;
            RefreshCompilingState();
            RefreshCompilationFailedState();

            Client.MasterServer.LoggedInStateChanged += RefreshLoggedInState;
            Client.MasterServer.LoginFailed += MasterServerLoginFailedHandler;
            Client.MasterServer.ConnectingFailed += MasterServerConnectingFailedHandler;
            Client.MasterServer.ConnectionStateChanged += MasterServerConnectionStateChangedHandler;
            Client.SteamApi.StateChanged += SteamServiceStateChanged;

            RefreshLoggedInState();

            if (Client.MasterServer.LastLoginErrorCode.HasValue)
            {
                MasterServerLoginFailedHandler(Client.MasterServer.LastLoginErrorCode.Value);
            }

            MasterServerConnectionStateChangedHandler();

            MainMenuMusicHelper.Init();
            GameplayMusicHelper.Init();
            ClientComponentCameraScreenShakes.Init();

            SoundConstants.ApplyConstants();
        }

        private static void ConnectToMasterServerIfNecessary()
        {
            if (Client.CurrentGame.ConnectionState != ConnectionState.Connected
                && Client.MasterServer.MasterServerConnectionState == ConnectionState.Disconnected
                && !MainMenuOverlay.IsHidden)
            {
                if (IsMasterServerCannotConnectDialogDisplayed())
                {
                    // do not autoconnect to the master server as player is expected to press Retry there
                    return;
                }

                Logger.Important(
                    "Connecting to MasterServer as it's necessary now (client is not connected to the game server and main menu overlay is not hidden)");
                Client.MasterServer.Connect();
            }
        }

        private static void CurrentGameServerConnectionChangedHandler()
        {
            Logger.Important("Current game connection state changed: " + Client.CurrentGame.ConnectionState);
            RefreshCurrentGameServerConnectionState();
            DisconnectMasterServerIfNotNecessary();

            // selects "Current game" tab when connected
            if (Client.CurrentGame.ConnectionState
                    is ConnectionState.Connecting
                    or ConnectionState.Connected)
            {
                ViewModelMainMenuOverlay.Instance.IsCurrentGameTabSelected = true;
            }
        }

        private static void GameServerConnectingFailedModsMissingHandler(IReadOnlyList<ServerModInfo> missingMods)
        {
            // ReSharper disable once CanExtractXamlLocalizableStringCSharp
            var missingModsListString = missingMods
                                        .Select(m => string.Format("[*][b]{0}[/b] - v{1}", m.Id, m.Version))
                                        .GetJoinedString(Environment.NewLine);

            var message = string.Format(DialogModsMissing_MessageFormat, missingModsListString);

            DialogWindow.ShowDialog(
                DialogModsMissing_Title,
                message,
                closeByEscapeKey: true,
                textAlignment: TextAlignment.Left);
        }

        private static bool IsMasterServerCannotConnectDialogDisplayed()
        {
            var dialog = (DialogWindow)cannotConnectToMasterServerDialog?.Target;
            return dialog is not null
                   && (dialog.GameWindow.State == GameWindowState.Opened
                       || dialog.GameWindow.State == GameWindowState.Opening);
        }

        private static void MasterServerConnectingFailedHandler()
        {
            if (IsMasterServerCannotConnectDialogDisplayed())
            {
                return;
            }

            if (Client.CurrentGame.ConnectionState == ConnectionState.Connected
                && MainMenuOverlay.IsHidden)
            {
                // already playing and main menu hidden - do not disturb the player!
                Logger.Important("Do not disturb the player");
                return;
            }

            string cancelText;
            Action cancelAction;
            if (Client.MasterServer.CurrentPlayerIsLoggedIn)
            {
                cancelText = CoreStrings.Button_StayOffline;
                cancelAction = () => { };
            }
            else
            {
                cancelText = CoreStrings.Button_Quit;
                cancelAction = () => Client.Core.Quit();
            }

            var dialog = DialogWindow.ShowDialog(
                title: DialogCannotConnectToTheMasterServer_Title,
                text: DialogCannotConnectToTheMasterServer_Message
                      + "[br]"
                      + "[br]"
                      + DialogCannotConnectToTheMasterServer_CanPlayOnLocalServer_Message,
                textAlignment: TextAlignment.Left,
                okText: CoreStrings.Button_Retry,
                cancelText: cancelText,
                okAction: () => Client.MasterServer.Connect(),
                cancelAction: cancelAction,
                zIndexOffset: 9002,
                autoWidth: true);

            // ensure the window is displayed
            dialog.UpdateLayout();
            dialog.Window.UpdateLayout();
            dialog.Window.Open();

            cannotConnectToMasterServerDialog = new WeakReference(dialog);
        }

        private static void MasterServerConnectionStateChangedHandler()
        {
            var state = Client.MasterServer.MasterServerConnectionState;
            MasterServerConnectionIndicator.MasterConnectionState = state;

            RefreshLoggedInState();

            ConnectToMasterServerIfNecessary();

            if (state == ConnectionState.Connected)
            {
                var dialog = (DialogWindow)cannotConnectToMasterServerDialog?.Target;
                if (dialog is not null
                    && (dialog.Window.State == GameWindowState.Opening
                        || dialog.Window.State == GameWindowState.Opened))
                {
                    dialog.Window.Close(DialogResult.Cancel);
                }
            }
        }

        private static void MasterServerLoginFailedHandler(MasterClientLoginErrorCode errorCode)
        {
            RefreshLoggedInState();
            MasterLoginFailedDialogHelper.ShowDialog(errorCode);
        }

        private static void RefreshCompilationFailedState()
        {
            CompilationSplashScreenFailedCompilation.IsDisplayed = Client.Core.IsCompilationFailed;
        }

        private static void RefreshCompilingState()
        {
            CompilationSplashScreen.IsDisplayed = Client.Core.IsCompiling;
        }

        private static void RefreshCurrentGameServerConnectionState()
        {
            ClientCursorSystem.CurrentCursorId = CursorId.Default;

            var currentGameService = Client.CurrentGame;
            var isConnected = currentGameService.ConnectionState == ConnectionState.Connected;
            var isConnecting = currentGameService.ConnectionState == ConnectionState.Connecting;

            if (Api.IsEditor
                && !isConnected
                && !isConnecting
                && Client.MasterServer.CurrentPlayerIsLoggedIn)
            {
                Logger.Info("Editor mode: automatically connect to the local game server");
                currentGameService.ConnectToServer(new ServerAddress());
                isConnecting = true;
            }

            if (isConnecting)
            {
                // ReSharper disable once CanExtractXamlLocalizableStringCSharp
                LoadingSplashScreenManager.Show("connecting to the server");
                MainMenuOverlay.IsHidden = true;
                return;
            }

            if (isConnected
                && !Client.Scene.IsEverythingLoaded)
            {
                // ensure it's shown
                // ReSharper disable once CanExtractXamlLocalizableStringCSharp
                LoadingSplashScreenManager.Show("not everything is loaded yet");
                // auto-hide
                LoadingSplashScreenManager.Hide();
                return;
            }

            LoadingSplashScreenManager.Hide();

            if (isConnected)
            {
                // connected and everything is loaded - can play now
                return;
            }

            // not connected to the game server
            // force menu overlay in non-game mode
            MainMenuOverlay.IsHidden = false;
        }

        private static void RefreshLoggedInState()
        {
            var isLoggedIn = Client.MasterServer.CurrentPlayerIsLoggedIn;

            if (isLoggedIn)
            {
                MenuLogin.SetDisplayed(MenuLoginMode.None);
                RefreshCurrentGameServerConnectionState();
            }
            else
            {
                var connectionState = Client.MasterServer.MasterServerConnectionState;

                if (Client.SteamApi.IsSteamClient
                    && Client.SteamApi.State == SteamApiState.Connecting)
                {
                    connectionState = ConnectionState.Connecting;
                }

                MenuLogin.SetDisplayed(connectionState == ConnectionState.Connecting
                                           ? MenuLoginMode.Connecting
                                           : MenuLoginMode.Login);
            }
        }

        private static void SteamServiceStateChanged()
        {
            RefreshLoggedInState();
        }
    }
}