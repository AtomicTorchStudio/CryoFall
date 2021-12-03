// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Admin
{
    using AtomicTorch.CBND.CoreMod.Events;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ConsoleAdminSkipBossEventStartDelay : BaseConsoleCommand
    {
        public override string Description =>
            "Skips the boss event start delay (removes the barrier and immediately spawns the boss) for all active boss events.";
 
        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "admin.skipBossEventStartDelay";

        public string Execute()
        {
            using var tempAllBossEvents =
                Api.Shared.WrapInTempList(
                    Server.World.GetGameObjectsOfProto<ILogicObject, ProtoEventBoss>());

            foreach (var worldEvent in tempAllBossEvents.AsList())
            {
                var publicState = worldEvent.GetPublicState<EventBossPublicState>();
                var protoEventBoss = ((ProtoEventBoss)worldEvent.ProtoLogicObject);
                publicState.EventEndTime = Server.Game.FrameTime
                                           + protoEventBoss.EventDurationWithoutDelay.TotalSeconds;
            }

            return "Done!";
        }
    }
}