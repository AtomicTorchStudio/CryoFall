// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Mod
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.ServerPlayerAccess;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ConsoleModWhiteListPrint : BaseConsoleCommand
    {
        public override string Description =>
            "Prints the whitelist content.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "mod.whiteList.print";

        public string Execute()
        {
            var list = ServerPlayerAccessSystem.GetWhiteList().ToList();
            if (list.Count == 0)
            {
                return "Whitelist is empty";
            }

            return "Whitelisted players:"
                   + Environment.NewLine
                   + list.GetJoinedString(Environment.NewLine);
        }
    }
}