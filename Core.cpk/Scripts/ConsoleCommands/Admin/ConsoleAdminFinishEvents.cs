namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Admin
{
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Events;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.Logic;

    public class ConsoleAdminFinishEvents : BaseConsoleCommand
    {
        public override string Description => "Finishes all the active world events.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "admin.finishEvents";

        public string Execute()
        {
            var events = Server.World.GetGameObjectsOfProto<ILogicObject, IProtoEvent>().ToList();

            foreach (var worldEvent in events)
            {
                worldEvent.GetPublicState<EventPublicState>()
                          .EventEndTime = 0;
            }

            return "All events are marked for expiration and should finish as soon as possible.";
        }
    }
}