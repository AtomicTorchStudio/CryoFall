// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Mod
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.ServerPlayerAccess;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ConsoleModKickListPrint : BaseConsoleCommand
    {
        public override string Description =>
            "Prints the kicked players list content.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerModerator;

        public override string Name => "mod.kickList.print";

        public string Execute()
        {
            var list = ServerPlayerAccessSystem.GetKickList().ToList();
            if (list.Count == 0)
            {
                return "Kick list is empty";
            }

            return "Kicked players:"
                   + Environment.NewLine
                   + list.GetJoinedString(Environment.NewLine);
        }
    }
}