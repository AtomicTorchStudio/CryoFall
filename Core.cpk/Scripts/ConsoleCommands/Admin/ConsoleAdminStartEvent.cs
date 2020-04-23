// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Admin
{
    using AtomicTorch.CBND.CoreMod.Events.Base;
    using AtomicTorch.CBND.CoreMod.Systems.Console;

    public class ConsoleAdminStartEvent : BaseConsoleCommand
    {
        public override string Alias => "startEvent";

        public override string Description => "Creates and starts an event.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "admin.startEvent";

        public string Execute(IProtoEvent protoEvent)
        {
            if (!protoEvent.ServerIsTriggerAllowed(null))
            {
                return "Event cannot be started - ServerIsTriggerAllowed() check failed, see the error in the log";
            }

            protoEvent.ServerForceCreateAndStart();
            return $"Event started: {protoEvent}";
        }
    }
}