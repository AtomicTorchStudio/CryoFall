// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Debug
{
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ConsoleDebugTestNotification : BaseConsoleCommand
    {
        public override string Description => "Displays notifications for debug purposes.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.Client;

        public override string Name => "debug.testNotification";

        public string Execute(
            string title = "Notification title",
            string message = "Notification message text")
        {
            var sound = new[] { NotificationColor.Good, NotificationColor.Neutral, NotificationColor.Bad }
                .TakeByRandom();
            NotificationSystem.ClientShowNotification(title, message, sound, playSound: true);
            return null;
        }
    }
}