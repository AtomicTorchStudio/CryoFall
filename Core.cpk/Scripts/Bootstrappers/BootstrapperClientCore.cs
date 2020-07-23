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

        public override void ClientInitialize()
        {
            if (!Api.IsEditor
                && !Api.Shared.IsRequiresLanguageSelection)
            {
                // force master server connection in non-game mode (in non-Editor only)
                Api.Client.MasterServer.Connect();
            }

            Client.Core.SetLoadingSplashScreenManager(LoadingSplashScreenManager.Instance);

            Client.CurrentGame.ConnectionStateChanged += RefreshCurrentGameServerConnectionState;
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
            Client.MasterServer.ConnectionStateChanged += RefreshMasterServerConnectionState;
            Client.SteamApi.StateChanged += SteamServiceStateChanged;

            RefreshLoggedInState();

            if (Client.MasterServer.LastLoginErrorCode.HasValue)
            {
                MasterServerLoginFailedHandler(Client.MasterServer.LastLoginErrorCode.Value);
            }

            RefreshMasterServerConnectionState();

            MainMenuMusicHelper.Init();
            GameplayMusicHelper.Init();
            ClientComponentCameraScreenShakes.Init();

            SoundConstants.ApplyConstants();
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

        private static void MasterServerConnectingFailedHandler()
        {
            var dialog = (DialogWindow)cannotConnectToMasterServerDialog?.Target;
            if (dialog != null
                && dialog.GameWindow.State == GameWindowState.Opened)
            {
                return;
            }

            string cancelText;
            Action cancelAction;
            if (Api.Client.MasterServer.CurrentPlayerIsLoggedIn)
            {
                cancelText = CoreStrings.Button_Cancel;
                cancelAction = () => { };
            }
            else
            {
                cancelText = CoreStrings.Button_Quit;
                cancelAction = () => Client.Core.Quit();
            }

            dialog = DialogWindow.ShowDialog(
                title: DialogCannotConnectToTheMasterServer_Title,
                text: DialogCannotConnectToTheMasterServer_Message,
                textAlignment: TextAlignment.Left,
                okText: CoreStrings.Button_Retry,
                cancelText: cancelText,
                okAction: () => Client.MasterServer.Connect(),
                cancelAction: cancelAction,
                zIndexOffset: 9002,
                autoWidth: true);

            cannotConnectToMasterServerDialog = new WeakReference(dialog);
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

            MainMenuOverlay.UpdateActiveTab();

            var currentGameService = Client.CurrentGame;
            var isConnected = currentGameService.ConnectionState == ConnectionState.Connected;
            var isConnecting = currentGameService.ConnectionState == ConnectionState.Connecting;

            if (!isConnected
                && !isConnecting
                && Client.MasterServer.CurrentPlayerIsLoggedIn)
            {
                var isEditor = Api.IsEditor;
                if (isEditor || Api.Client.Input.IsKeyHeld(InputKey.F1))
                {
                    if (isEditor)
                    {
                        Logger.Important("Editor mode: automatically connect to the local game server");
                    }

                    currentGameService.ConnectToServer(new ServerAddress());
                    isConnecting = true;
                }
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
                // ensure it's allowed to hide (when everything will be loaded)
                LoadingSplashScreenManager.Hide();
                MainMenuOverlay.IsHidden = true;
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

                if (Api.Client.SteamApi.IsSteamClient
                    && Api.Client.SteamApi.State == SteamApiState.Connecting)
                {
                    connectionState = ConnectionState.Connecting;
                }

                MenuLogin.SetDisplayed(connectionState == ConnectionState.Connecting
                                           ? MenuLoginMode.Connecting
                                           : MenuLoginMode.Login);
            }
        }

        private static void RefreshMasterServerConnectionState()
        {
            var state = Client.MasterServer.MasterServerConnectionState;
            MasterServerConnectionIndicator.MasterConnectionState = state;

            RefreshLoggedInState();
        }

        private static void SteamServiceStateChanged()
        {
            RefreshLoggedInState();
        }
    }
}