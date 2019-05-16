// ReSharper disable CanExtractXamlLocalizableStringCSharp
namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Admin
{
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    public class ConsoleAdminNotifyAll : BaseConsoleCommand
    {
        public override string Description =>
            @"Notifies all players on the server.
              Important: wrap the ""message text in quotes""!";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "admin.notifyAll";

        public string Execute(string message)
        {
            this.CallClient(
                Server.Characters.EnumerateAllPlayerCharacters(onlyOnline: true),
                _ => _.ClientRemote_ShowNotification(message));
            return null;
        }

        private void ClientRemote_ShowNotification(string message)
        {
            NotificationSystem.ClientShowNotification(
                title: null,
                message: message,
                autoHide: false);
        }
    }
}