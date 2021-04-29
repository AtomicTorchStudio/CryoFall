namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Debug
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ConsoleDebugListSelectedCharacterOrigins : BaseConsoleCommand
    {
        public override string Description =>
            "Lists how many players selected each character origin.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "debug.listSelectedCharacterOrigins";

        public string Execute()
        {
            var origins = Server.Characters.EnumerateAllPlayerCharacters(onlyOnline: false)
                                .GroupBy(p => PlayerCharacter.GetPrivateState(p).Origin)
                                .Where(g => g.Key is not null)
                                .OrderBy(g => g.Key.ShortId);
            return "Selected origins:"
                   + Environment.NewLine
                   + origins.Select(g => $" * {g.Key.ShortId}: {g.Count()} player(s)")
                            .GetJoinedString(Environment.NewLine);
        }
    }
}