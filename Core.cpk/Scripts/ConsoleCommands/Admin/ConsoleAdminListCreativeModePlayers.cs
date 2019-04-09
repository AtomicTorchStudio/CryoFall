// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Admin
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.Creative;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ConsoleAdminListCreativeModePlayers : BaseConsoleCommand
    {
        public override string Description
            => "Lists players currently in creative mode";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        //todo: This console command doesn't follow our current conventions. Maybe we can do something about it?

        public override string Name => "admin.listCreativeModePlayers";

        public string Execute()
        {
            var list = CreativeModeSystem.ServerUsersInCreativeMode;
            return "Players in creative mode: "
                   + Environment.NewLine
                   + list.Select(id => " * " + id)
                         .GetJoinedString(Environment.NewLine);
        }
    }
}