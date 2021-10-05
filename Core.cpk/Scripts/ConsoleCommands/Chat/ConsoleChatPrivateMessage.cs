// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Chat
{
    using AtomicTorch.CBND.CoreMod.Systems.Chat;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Chat;

    public class ConsoleChatPrivateMessage : BaseConsoleCommand
    {
        public override string Description =>
            "Opens a private chat with the specified player (useful when this player is offline and you cannot find another way to contact).";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.Client;

        public override string Name => "chat.openPrivateChat";

        public string Execute(string playerName)
        {
            OpenChatAsync();
            return null;

            async void OpenChatAsync()
            {
                var isChatOpened = await ChatSystem.ClientOpenPrivateChat(playerName);
                if (isChatOpened)
                {
                    NotificationSystem.ClientShowNotification(null,
                                                              "Private chat opened with " + playerName,
                                                              NotificationColor.Good);
                }
                else
                {
                    NotificationSystem.ClientShowNotification(null,
                                                              "Player not found: " + playerName,
                                                              NotificationColor.Bad);
                }

                if (ConsoleControl.Instance?.IsDisplayed ?? false)
                {
                    // hide the console so the player can see the notification
                    ConsoleControl.Instance.IsDisplayed = false;
                }

                if (isChatOpened)
                {
                    ChatPanel.Instance.Open();
                }
            }
        }
    }
}