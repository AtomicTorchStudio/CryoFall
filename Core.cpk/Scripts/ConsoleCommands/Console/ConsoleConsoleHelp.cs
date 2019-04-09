// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Console
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.ServerOperator;

    public class ConsoleConsoleHelp : BaseConsoleCommand
    {
        public override string Alias => "help";

        public override string Description => "Prints information about all the available console commands.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ClientAndServerOperatorOnly;

        public override string Name => "console.help";

        public string Execute(
            [CustomSuggestions(nameof(GetCommandNameSuggestions))]
            string searchCommand = null)
        {
            IEnumerable<BaseConsoleCommand> allCommands = ConsoleCommandsSystem.AllCommands;

            if (this.ExecutionContextCurrentCharacter != null
                && !ServerOperatorSystem.SharedIsOperator(this.ExecutionContextCurrentCharacter))
            {
                // not a server operator - exclude server operator commands
                allCommands = allCommands.Where(c => c.Kind != ConsoleCommandKinds.ServerOperator);
            }

            var sb = new StringBuilder(capacity: 2047);
            sb.AppendLine().AppendLine();

            if (!string.IsNullOrEmpty(searchCommand))
            {
                var foundCommandsList =
                    allCommands.Where(
                                   c => c.Name.StartsWith(searchCommand, StringComparison.OrdinalIgnoreCase)
                                        || c.Alias != null
                                        && c.Alias.StartsWith(searchCommand, StringComparison.OrdinalIgnoreCase))
                               .ToList();
                if (foundCommandsList.Count == 0)
                {
                    sb.Append("No commands found.");
                    return sb.ToString();
                }

                allCommands = foundCommandsList;
            }

            //AppendLegend(sb);
            //sb.AppendLine();
            sb.AppendLine("Commands: ");

            foreach (var consoleCommand in allCommands)
            {
                string prefix;
                switch (consoleCommand.Kind)
                {
                    // add server suffix for server commands
                    case ConsoleCommandKinds.ServerEveryone:
                    case ConsoleCommandKinds.ServerOperator:
                        prefix = "/";
                        break;

                    case ConsoleCommandKinds.ClientAndServerEveryone:
                    case ConsoleCommandKinds.ClientAndServerOperatorOnly:
                        prefix = "(/)";
                        break;

                    default:
                        prefix = string.Empty;
                        break;
                }

                AppendCommandInfo(sb, consoleCommand, prefix);
            }

            return sb.ToString();
        }

        private static void AppendCommandInfo(StringBuilder sb, BaseConsoleCommand consoleCommand, string prefix)
        {
            if (consoleCommand.Alias != null)
            {
                sb.Append(prefix)
                  .Append(consoleCommand.Alias)
                  .AppendLine();
            }

            sb.Append(prefix)
              .Append(consoleCommand.Name);

            sb.AppendLine();

            sb.Append("* Description: ")
              .AppendLine(consoleCommand.Description);

            sb.Append("* Type: ")
              .Append(GetTypeString(consoleCommand.Kind))
              .AppendLine()
              .AppendLine("* Usage: ");

            var isServer = IsServer;

            if (consoleCommand.Alias != null)
            {
                PrintUsageExample(consoleCommand.Alias);
            }

            PrintUsageExample(consoleCommand.Name);
            sb.AppendLine();

            void PrintUsageExample(string title)
            {
                foreach (var variant in consoleCommand.Variants)
                {
                    sb.Append("  ");

                    if (isServer
                        || (consoleCommand.Kind & ConsoleCommandKinds.ServerEveryone) != 0
                        || (consoleCommand.Kind & ConsoleCommandKinds.ServerOperator) != 0)
                    {
                        sb.Append(ConsoleCommandsSystem.ServerConsoleCommandPrefixOnClient);
                    }

                    sb.Append(title);

                    var parameters = variant.Parameters;
                    foreach (var commandParameter in parameters)
                    {
                        sb.Append(' ')
                          .Append(commandParameter.Name);
                    }

                    sb.AppendLine();
                }
            }
        }

        private static void AppendLegend(StringBuilder sb)
        {
            sb.AppendLine("Legend: ")
              .AppendLine(" <angle brackets>  - this is a required argument")
              .AppendLine(" [square brackets] - this is an optional argument")
              .AppendLine(" plain text        - enter this literally, exactly as shown")
              .AppendLine(" x|y|z             - input one of the options");
        }

        private static IEnumerable<string> GetCommandNameSuggestions(string startsWith)
        {
            foreach (var command in ConsoleCommandsSystem.SharedGetCommandNamesSuggestions(startsWith))
            {
                yield return command.Name;

                if (command.Alias != null)
                {
                    yield return command.Alias;
                }
            }
        }

        private static string GetTypeString(ConsoleCommandKinds kind)
        {
            switch (kind)
            {
                case ConsoleCommandKinds.Client:
                    return "client";
                case ConsoleCommandKinds.ServerEveryone:
                    return "server";
                case ConsoleCommandKinds.ServerOperator:
                    return "server (operator only)";
                case ConsoleCommandKinds.ClientAndServerEveryone:
                    return "client & server";
                case ConsoleCommandKinds.ClientAndServerOperatorOnly:
                    return "client & server (operator only)";
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
            }
        }
    }
}