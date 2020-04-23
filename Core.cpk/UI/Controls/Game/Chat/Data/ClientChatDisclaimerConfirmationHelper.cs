namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Chat.Data
{
    using System.Collections.Generic;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.CBND.GameApi.ServicesClient.Servers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public static class ClientChatDisclaimerConfirmationHelper
    {
        private static readonly IClientStorage Storage;

        static ClientChatDisclaimerConfirmationHelper()
        {
            Storage = Api.Client.Storage.GetStorage("Servers/ConfirmedChatUsage");
            Storage.RegisterType(typeof(ServerAddress));
            Storage.RegisterType(typeof(AtomicGuid));
        }

        public static bool IsChatAllowedForCurrentServer
        {
            get
            {
                if (Api.IsEditor)
                {
                    return true;
                }

                if (!Storage.TryLoad(
                        out Dictionary<ServerAddress, bool> dictionary))
                {
                    // chat usage is not confirmed
                    // a dialog window will be displayed when it's opened next time
                    return true;
                }

                var serverAddress = Api.Client.CurrentGame.ServerInfo.ServerAddress;
                return dictionary.TryGetValue(serverAddress, out bool isAllowed)
                       && isAllowed;
            }
        }

        public static bool GetIsNeedToDisplayDisclaimerForCurrentServer(out bool isChatAllowed)
        {
            if (Api.IsEditor)
            {
                isChatAllowed = true;
                return false;
            }

            if (!Storage.TryLoad(
                    out Dictionary<ServerAddress, bool> dictionary))
            {
                isChatAllowed = true;
                return true;
            }

            var serverAddress = Api.Client.CurrentGame.ServerInfo.ServerAddress;
            return !dictionary.TryGetValue(serverAddress, out isChatAllowed);
        }

        public static void OpenDisclaimerDialogIfNecessary(bool askAgain = false)
        {
            if (!GetIsNeedToDisplayDisclaimerForCurrentServer(out var isChatAllowed)
                && (isChatAllowed || !askAgain))
            {
                return;
            }

            // close the opened chat instantly
            ClientTimersSystem.AddAction(0,
                                         () => ChatPanel.Instance.Close());

            // display a disclaimer dialog window
            var isOfficialServer = Api.Client.CurrentGame.IsConnectedToOfficialServer;

            var sb = new StringBuilder();
            sb.Append(isOfficialServer
                          ? CoreStrings.ChatDisclaimer_Title_Official
                          : CoreStrings.ChatDisclaimer_Title_Community);
            sb.Append("[br]")
              .Append("[br]");

            sb.Append(isOfficialServer
                          ? CoreStrings.ChatDisclaimer_NotModerated_Official
                          : CoreStrings.ChatDisclaimer_NotModerated_Community);

            sb.Append("[br]")
              .Append("[br]")
              .Append(CoreStrings.ChatDisclaimer_UseBlock)
              .Append("[br]");

            var checkbox = new CheckBox
            {
                Content = CoreStrings.ChatDisclaimer_Checkbox,
                Focusable = false
            };

            var stackPanel = new StackPanel() { Orientation = Orientation.Vertical };
            stackPanel.Children.Add(new FormattedTextBlock() { Content = sb.ToString() });
            stackPanel.Children.Add(checkbox);

            var dialogWindow = DialogWindow.ShowDialog(
                title: null,
                content: stackPanel,
                okText: CoreStrings.ChatDisclaimer_Button_EnableChat,
                cancelText: CoreStrings.ChatDisclaimer_Button_DisableChat,
                okAction: () =>
                          {
                              SetConfirmedForCurrentServer(isAllowed: true);
                              ChatPanel.Instance.RefreshState();
                              ChatPanel.Instance.Open();
                          },
                cancelAction:
                () =>
                {
                    SetConfirmedForCurrentServer(isAllowed: false);
                    ChatPanel.Instance.RefreshState();
                });

            dialogWindow.Window.FocusOnControl = null;

            BindingOperations.SetBinding(
                dialogWindow.ButtonOk,
                UIElement.IsEnabledProperty,
                new Binding(ToggleButton.IsCheckedProperty.Name)
                {
                    Source = checkbox,
                    Mode = BindingMode.OneWay
                });
        }

        public static void SetConfirmedForCurrentServer(bool isAllowed)
        {
            if (!Storage.TryLoad(
                    out Dictionary<ServerAddress, bool> dictionary))
            {
                dictionary = new Dictionary<ServerAddress, bool>();
            }

            var serverAddress = Api.Client.CurrentGame.ServerInfo.ServerAddress;
            dictionary[serverAddress] = isAllowed;
            Storage.Save(dictionary);
        }
    }
}