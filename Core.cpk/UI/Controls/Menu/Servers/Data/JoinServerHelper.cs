namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data
{
    using System;
    using AtomicTorch.CBND.CoreMod.Helpers.Client.Server;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Helpers;
    using AtomicTorch.CBND.GameApi.Scripting;

    public static class JoinServerHelper
    {
        public const string DialogEditorMode_Message = "Cannot connect to any server in Editor mode.";

        public const string DialogEditorMode_Title = "You're running the game in Editor mode.";

        public const string DialogIncompatibleServer_Advice_ConnectNewServer =
            "Please connect to the server of the newer version.";

        public const string DialogIncompatibleServer_Advice_UpdateClient =
            "Please update your game client to the latest version.";

        public const string DialogIncompatibleServer_MessageFormat =
            @"[br]Server version {0}
              [br]Your game version {1}";

        public const string DialogIncompatibleServer_Title = "Incompatible server";

        public const string DialogIncompatibleServer_VersionTitle_Newer =
            "This game server is running a newer version of the game";

        public const string DialogIncompatibleServer_VersionTitle_Older =
            "This game server is running an older version of the game";

        public const string DialogServerInaccessible =
            @"[b]Internet connection problem[/b]
              [br]Game server cannot be reached due to unstable network connection or failed routing of your internet service provider.";

        public static async void ExecuteCommandJoinServer(ViewModelServerInfo serverInfo)
        {
            if (Api.IsEditor)
            {
                DialogWindow.ShowDialog(
                    DialogEditorMode_Title,
                    DialogEditorMode_Message);
                return;
            }

            var address = serverInfo.Address;
            if (address == default)
            {
                // should be impossible
                throw new Exception("Bad server address");
            }

            if (serverInfo.IsInaccessible)
            {
                var title = serverInfo.IsOfficial
                                ? null
                                : GameServerLoginFailedDialogHelper.TitleDefault;

                var message = serverInfo.IsOfficial
                                  ? DialogServerInaccessible
                                  : GameServerLoginFailedDialogHelper.ServerUnreachable;

                DialogWindow.ShowDialog(
                    title: title,
                    message,
                    okAction: () =>
                              {
                                  if (!serverInfo.IsInaccessible)
                                  {
                                      // became accessible while dialog was displayed
                                      ExecuteCommandJoinServer(serverInfo);
                                      return;
                                  }

                                  // still inaccessible
                                  serverInfo.CommandRefresh?.Execute(serverInfo);
                                  serverInfo.RefreshAndDisplayPleaseWaitDialog(
                                      onInfoReceivedOrCannotReach: () => ExecuteCommandJoinServer(serverInfo));
                              },
                    cancelAction: () => { },
                    okText: CoreStrings.Button_Retry,
                    cancelText: CoreStrings.Button_Cancel,
                    closeByEscapeKey: true,
                    zIndexOffset: 100000);
                return;
            }

            if (!serverInfo.IsInfoReceived)
            {
                serverInfo.RefreshAndDisplayPleaseWaitDialog(
                    onInfoReceivedOrCannotReach: () => ExecuteCommandJoinServer(serverInfo));
                return;
            }

            if (!(serverInfo.IsCompatible ?? false))
            {
                var serverVersion = serverInfo.Version;
                var clientVersion = Api.Shared.GameVersionNumber;
                string problemTitle, advice;
                if (serverVersion > clientVersion)
                {
                    problemTitle = DialogIncompatibleServer_VersionTitle_Newer;
                    advice = DialogIncompatibleServer_Advice_UpdateClient;
                }
                else
                {
                    problemTitle = DialogIncompatibleServer_VersionTitle_Older;
                    advice = DialogIncompatibleServer_Advice_ConnectNewServer;
                }

                var message = problemTitle
                              // ReSharper disable once CanExtractXamlLocalizableStringCSharp
                              + ":[br]"
                              + string.Format(DialogIncompatibleServer_MessageFormat,
                                              serverVersion,
                                              clientVersion)
                              // ReSharper disable once CanExtractXamlLocalizableStringCSharp
                              + "[br][br]"
                              + advice;

                DialogWindow.ShowDialog(
                    DialogIncompatibleServer_Title,
                    message,
                    closeByEscapeKey: true,
                    zIndexOffset: 100000);
                return;
            }

            if (Api.Client.MasterServer.IsDemoVersion
                && !serverInfo.IsOfficial
                && !serverInfo.IsFeatured)
            {
                DialogWindow.ShowDialog(
                    CoreStrings.Demo_Title,
                    CoreStrings.Demo_OnlyOfficialServers,
                    okText: CoreStrings.Demo_Button_BuyGameOnSteam,
                    okAction: () => Api.Client.SteamApi.OpenBuyGamePage(),
                    cancelText: CoreStrings.Button_Cancel,
                    cancelAction: () => { },
                    focusOnCancelButton: true,
                    closeByEscapeKey: true,
                    zIndexOffset: 100000);
                return;
            }

            await JoinServerAgreementDialogHelper.Display();

            Api.Client.CurrentGame.ConnectToServer(address);
        }
    }
}