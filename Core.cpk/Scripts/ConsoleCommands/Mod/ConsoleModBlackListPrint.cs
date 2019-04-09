// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Mod
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.ServerPlayerAccess;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ConsoleModBlackListPrint : BaseConsoleCommand
    {
        public override string Description =>
            "Prints the blacklist content.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "mod.blackList.print";

        public string Execute()
        {
            var list = ServerPlayerAccessSystem.GetBlackList().ToList();
            if (list.Count == 0)
            {
                return "Blacklist is empty";
            }

            return "Blacklisted players:"
                   + Environment.NewLine
                   + list.GetJoinedString(Environment.NewLine);
        }
    }
}