namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Admin
{
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Events.Base;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.Logic;

    public class ConsoleAdminClearEventPastLocations : BaseConsoleCommand
    {
        public override string Description => "Clear all the past locations for all the events (so new events could happen there again).";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "admin.clearEventPastLocations";

        public string Execute()
        {
            ServerEventLocationManager.Clear();
            return "Clear successful.";
        }
    }
}