// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Mod
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.ServerPlayerAccess;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ConsoleModMuteListPrint : BaseConsoleCommand
    {
        public override string Description =>
            "Prints the muted players list content.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "mod.muteList.print";

        public string Execute()
        {
            var list = ServerPlayerMuteSystem.GetMuteList().ToList();
            if (list.Count == 0)
            {
                return "Mute list is empty";
            }

            return "Muted players:"
                   + Environment.NewLine
                   + list.GetJoinedString(Environment.NewLine);
        }
    }
}