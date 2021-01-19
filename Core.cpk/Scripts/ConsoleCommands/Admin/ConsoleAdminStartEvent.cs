// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Admin
{
    using AtomicTorch.CBND.CoreMod.Events;
    using AtomicTorch.CBND.CoreMod.Systems.Console;

    public class ConsoleAdminStartEvent : BaseConsoleCommand
    {
        public override string Alias => "startEvent";

        public override string Description =>
            "Creates and starts an event. Use autocomplete to browse all the available events. To finish all the ongoing events you can use /admin.finishEvents";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "admin.startEvent";

        public string Execute(IProtoEvent eventName)
        {
            if (!eventName.ServerIsTriggerAllowed(null))
            {
                return "Event cannot be started - ServerIsTriggerAllowed() check failed, see the error in the log";
            }

            eventName.ServerForceCreateAndStart();
            return $"Event started: {eventName}";
        }
    }
}