// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Admin
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.ServerModerator;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ConsoleAdminModeratorList : BaseConsoleCommand
    {
        public override string Alias => "moderatorList";

        public override string Description
            => "Lists server moderators.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "admin.moderator.list";

        public string Execute()
        {
            var list = ServerModeratorSystem.ServerModeratorsList;
            return "Server moderators: "
                   + Environment.NewLine
                   + list.Select(id => " * " + id)
                         .GetJoinedString(Environment.NewLine);
        }
    }
}