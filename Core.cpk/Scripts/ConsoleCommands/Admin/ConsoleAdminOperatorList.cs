// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Admin
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.ServerOperator;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ConsoleAdminOperatorList : BaseConsoleCommand
    {
        public override string Alias => "opList";

        public override string Description
            => "Lists server operators.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "admin.operator.list";

        public string Execute()
        {
            var list = ServerOperatorSystem.ServerOperatorsList;
            return "Server operators: "
                   + Environment.NewLine
                   + list.Select(id => " * " + id)
                         .GetJoinedString(Environment.NewLine);
        }
    }
}