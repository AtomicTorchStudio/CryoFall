// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Admin
{
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    public class ConsoleAdminNotifyPlayer : BaseConsoleCommand
    {
        public override string Description =>
            @"Notifies a player on the server.
              Important: wrap the ""message text in quotes""!";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "admin.notifyPlayer";

        public string Execute(ICharacter player, string messageInQuotes)
        {
            ConsoleAdminNotifySystem.Instance.CallClient(
                player,
                _ => _.ClientRemote_ShowNotification(messageInQuotes));
            return null;
        }

        private class ConsoleAdminNotifySystem : ProtoSystem<ConsoleAdminNotifySystem>
        {
            public void ClientRemote_ShowNotification(string message)
            {
                NotificationSystem.ClientShowNotification(
                                      title: null,
                                      message: message)
                                  .HideAfterDelay(10 * 60);
            }
        }
    }
}