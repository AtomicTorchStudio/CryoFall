namespace AtomicTorch.CBND.CoreMod.Helpers.Client.Server
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.ServicesClient;

    public static class GameServerLoginFailedDialogHelper
    {
        public const string IncompatibleNetworkSchemeVersionHash =
            @"Incompatible network version.
              [br]Perhaps you and the server are using different mods.";

        public const string MasterServerDisallowed =
            "Master Server refused this connection. Please try again or restart the game.";

        public const string ReasonUnknown =
            "Unknown error. Please try again.";

        public const string ServerFull =
            "Server is at full capacity at the moment. Please wait for other players to leave, then try to join the server again.";

        public const string ServerUnreachable =
            "Game server is currently unreachable. Possible reasons: server reboot, server shutdown, firewall configuration, network problems, etc. Please try again in a few minutes.";

        public const string ShutdownScheduled = "The game server is currently rebooting or is about to shut down.";

        public const string TitleConnectionRejectedByServer = "Connection rejected";

        public const string TitleDefault = "Cannot connect to the game server";

        public static void ShowDialog(
            GameServerConnectingFailedReason reason,
            string scriptingRestrictionMessage)
        {
            var errorTitle = TitleDefault;
            string errorMessage;
            switch (reason)
            {
                case GameServerConnectingFailedReason.ScriptingRestriction:
                    errorTitle = TitleConnectionRejectedByServer;
                    if (!scriptingRestrictionMessage.EndsWith("."))
                    {
                        scriptingRestrictionMessage += '.';
                    }

                    errorMessage = scriptingRestrictionMessage;
                    break;

                case GameServerConnectingFailedReason.ServerUnreachable:
                    errorMessage = ServerUnreachable;
                    break;

                case GameServerConnectingFailedReason.MasterServerDisallow:
                    errorMessage = MasterServerDisallowed;
                    break;

                case GameServerConnectingFailedReason.ShutdownScheduled:
                    // ReSharper disable once CanExtractXamlLocalizableStringCSharp
                    errorMessage = ShutdownScheduled
                                   + "[br]"
                                   + scriptingRestrictionMessage;
                    break;

                case GameServerConnectingFailedReason.IncompatibleNetworkSchemeVersionHash:
                    errorMessage = IncompatibleNetworkSchemeVersionHash;
                    break;

                case GameServerConnectingFailedReason.ServerFull:
                    errorMessage = ServerFull;
                    break;

                case GameServerConnectingFailedReason.Unknown:
                default:
                    errorMessage = ReasonUnknown;
                    break;
            }

            DialogWindow.ShowDialog(errorTitle,
                                    errorMessage,
                                    zIndexOffset: 9002);
        }
    }
}