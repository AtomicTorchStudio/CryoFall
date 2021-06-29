namespace AtomicTorch.CBND.CoreMod.Systems.Console
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.ServerModerator;
    using AtomicTorch.CBND.CoreMod.Systems.ServerOperator;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesClient;

    public class ConsoleCommandsSystem : ProtoSystem<ConsoleCommandsSystem>
    {
        /// <summary>
        /// Special prefix for server console command executed only client. If command starts with this prefix, it will be trimmed
        /// and remaining text will be send to the server for execution.
        /// </summary>
        public const char ServerConsoleCommandPrefixOnClient = '/';

        public static readonly int MaxSuggestions
            = IsClient || Api.IsEditor
                  ? 1000
                  // limit network bandwidth on suggestions
                  : 50;

        private static readonly string[] EmptySuggestionsArray = new string[0];

        private static IReadOnlyList<BaseConsoleCommand> allCommands;

        public delegate void ClientSuggestionsCallbackDelegate(
            ConsoleCommandVariant commandVariant,
            IReadOnlyList<string> suggestions,
            byte requestId);

        public static IReadOnlyList<BaseConsoleCommand> AllCommands => allCommands;

        public static ClientSuggestionsCallbackDelegate ClientSuggestionsCallback { get; set; }

        public override string Name => "Console commands system";

        public static void ClientSuggestAutocomplete(string text, ushort textPosition, byte requestId)
        {
            if (string.IsNullOrEmpty(text))
            {
                ClientSuggestionsCallback(null, null, requestId);
                return;
            }

            if (text[0] == ServerConsoleCommandPrefixOnClient)
            {
                if (Client.CurrentGame.ConnectionState != ConnectionState.Connected)
                {
                    Logger.Warning("Not connected to the game server, please don't type a server console command!");
                    return;
                }

                // get suggestions for this command on Server-side
                Instance.CallServer(_ => _.ServerRemote_GetSuggestions(text, textPosition, requestId));
                return;
            }

            // try getting suggestions for this command
            var byCharacter = IsClient
                                  ? Client.Characters.CurrentPlayerCharacter
                                  : null;

            var suggestions = SharedSuggestAutocomplete(
                text,
                byCharacter,
                textPosition,
                out var consoleCommandVariant);
            ClientSuggestionsCallback(consoleCommandVariant, suggestions, requestId);
        }

        /// <summary>
        /// Returns true if character has the Moderator role or it's null and we're on the server
        /// (it means the command is invoked from the server system console directly).
        /// </summary>
        public static bool ServerIsModeratorOrSystemConsole(ICharacter character)
        {
            return ServerModeratorSystem.SharedIsModerator(character)
                   || character is null && Api.IsServer;
        }

        /// <summary>
        /// Returns true if character has the Operator role or it's null and we're on the server
        /// (it means the command is invoked from the server system console directly).
        /// </summary>
        public static bool ServerIsOperatorOrSystemConsole(ICharacter character)
        {
            return ServerOperatorSystem.SharedIsOperator(character)
                   || character is null && Api.IsServer;
        }

        public static void ServerOnConsoleCommandResult(
            ICharacter byCharacter,
            BaseConsoleCommand command,
            string result)
        {
            Instance.CallClient(byCharacter,
                                _ => _.ClientRemote_ConsoleCommandResult(command.Name, result));
        }

        public static IReadOnlyList<string> ServerSuggestAutocomplete(
            string text,
            ICharacter byCharacter,
            ushort textPosition,
            out ConsoleCommandVariant consoleCommandVariant)
        {
            return SharedSuggestAutocomplete(text, byCharacter, textPosition, out consoleCommandVariant);
        }

        public static void SharedExecuteConsoleCommand(string text)
        {
            var byCharacter = IsClient
                                  ? Client.Characters.CurrentPlayerCharacter
                                  : null;

            SharedExecuteConsoleCommand(text, byCharacter);
        }

        public static void SharedFilterAvailableCommands(
            List<BaseConsoleCommand> suggestedCommands,
            bool isOperator,
            bool isModerator)
        {
            if (!isOperator)
            {
                // remove operator-only commands
                if (IsServer)
                {
                    suggestedCommands.RemoveAll(
                        sc => (sc.Kind & ConsoleCommandKinds.ServerOperator) != 0);
                }
                else // if client
                {
                    suggestedCommands.RemoveAll(
                        sc => (sc.Kind & ConsoleCommandKinds.ServerOperator) != 0
                              && (sc.Kind & ConsoleCommandKinds.Client) == 0);
                }
            }

            if (!isModerator)
            {
                // remove moderator-only commands
                if (IsServer)
                {
                    suggestedCommands.RemoveAll(
                        sc => (sc.Kind & ConsoleCommandKinds.ServerModerator) != 0);
                }
                else // if client
                {
                    suggestedCommands.RemoveAll(
                        sc => (sc.Kind & ConsoleCommandKinds.ServerModerator) != 0
                              && (sc.Kind & ConsoleCommandKinds.Client) == 0);
                }
            }
        }

        public static TConsoleCommand SharedGetCommand<TConsoleCommand>()
            where TConsoleCommand : BaseConsoleCommand, new()
        {
            var requiredType = typeof(TConsoleCommand);
            foreach (var command in allCommands)
            {
                if (command.GetType() == requiredType)
                {
                    return (TConsoleCommand)command;
                }
            }

            throw new Exception("Command not found: " + requiredType.FullName);
        }

        public static List<BaseConsoleCommand> SharedGetCommandNamesSuggestions(string startsWith)
        {
            IEnumerable<BaseConsoleCommand> commands = allCommands;

            if (startsWith.Length > 0
                && startsWith[0] == ServerConsoleCommandPrefixOnClient)
            {
                startsWith = startsWith.TrimStart(ServerConsoleCommandPrefixOnClient);
                commands = commands.Where(
                    // exclude client commands on Server-side
                    c => c.Kind != ConsoleCommandKinds.Client);
            }
            else if (IsClient)
            {
                commands = commands.Where(
                    // exclude server commands on Client-side
                    c => c.Kind != ConsoleCommandKinds.ServerEveryone
                         && c.Kind != ConsoleCommandKinds.ServerOperator
                         && c.Kind != ConsoleCommandKinds.ServerModerator);
            }

            return commands
                   .Where(
                       command => command.Name.StartsWith(startsWith, StringComparison.OrdinalIgnoreCase)
                                  || (command.Alias?.StartsWith(startsWith, StringComparison.OrdinalIgnoreCase)
                                      ?? false))
                   .ToList();
        }

        public static ConsoleCommandData SharedParseCommand(
            string text,
            out string commandName,
            ushort textPosition = 0)
        {
            if (string.IsNullOrEmpty(text))
            {
                commandName = string.Empty;
                return null;
            }

            ConsoleCommandParser.ParseCommandNameAndArguments(
                text,
                textPosition,
                out commandName,
                out var arguments,
                out var argumentIndexForSuggestion);

            var consoleCommand = SharedFindConsoleCommandByName(commandName);
            if (consoleCommand is null)
            {
                // unknown command
                return null;
            }

            var kind = consoleCommand.Kind;

            if (IsClient
                && (kind & ConsoleCommandKinds.Client) == 0)
            {
                // non-client command
                return null;
            }

            if (IsServer
                && (kind & ConsoleCommandKinds.ServerOperator) == 0
                && (kind & ConsoleCommandKinds.ServerEveryone) == 0
                && (kind & ConsoleCommandKinds.ServerModerator) == 0)
            {
                // non-server command
                return null;
            }

            return new ConsoleCommandData(consoleCommand, arguments, (byte)argumentIndexForSuggestion);
        }

        public void ServerOnConsoleCommandError(ICharacter byCharacter, string exceptionMessage)
        {
            this.CallClient(byCharacter, _ => _.ClientRemote_ConsoleCommandError(exceptionMessage));
        }

        protected override void PrepareSystem()
        {
            // populate commands
            var dictionary = new Dictionary<string, BaseConsoleCommand>(StringComparer.OrdinalIgnoreCase);
            IEnumerable<BaseConsoleCommand> commands = Api.Shared.FindScriptingTypes<BaseConsoleCommand>()
                                                          .Select(t => t.CreateInstance())
                                                          .ToArray();

            if (IsServer)
            {
                commands = commands.Where(
                    c => (c.Kind & ConsoleCommandKinds.ServerEveryone) != 0
                         || (c.Kind & ConsoleCommandKinds.ServerOperator) != 0
                         || (c.Kind & ConsoleCommandKinds.ServerModerator) != 0);
            }

            //// Commented out - client still need to know about server commands in order
            //// to provide proper auto-complete.
            ////else // if client
            ////{
            ////    commands = commands.Where(
            ////        c => c.CommandKind == ConsoleCommandKind.Shared
            ////             || c.CommandKind == ConsoleCommandKind.Client);
            ////}

            foreach (var command in commands)
            {
                command.InitializeIfRequired();

                var commandName = command.Name;
                var commandAlias = command.Alias;

                // register command name
                try
                {
                    dictionary.Add(commandName, command);
                }
                catch (ArgumentException)
                {
                    Logger.Error(
                        $"Command name={commandName} (class {command.Id}) is already used by class {dictionary[commandName].Id}");
                }

                if (commandAlias is null)
                {
                    continue;
                }

                // register alias
                try
                {
                    dictionary.Add(commandAlias, command);
                }
                catch (ArgumentException)
                {
                    Logger.Error(
                        $"Command alias={commandAlias} (class {command.Id}) is already used by class {dictionary[commandAlias].Id}");
                }
            }

            allCommands = dictionary.Values
                                    .Distinct()
                                    .OrderBy(c => c.Name)
                                    .ToList();

            if (IsServer)
            {
                Server.Core.OnConsoleCommand += this.ServerHandleSystemConsoleCommand;
            }
        }

        private static void SharedExecuteConsoleCommand(string text, ICharacter byCharacter)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            if (text[0] == ServerConsoleCommandPrefixOnClient)
            {
                if (IsClient)
                {
                    if (Client.CurrentGame.ConnectionState != ConnectionState.Connected)
                    {
                        Logger.Warning("Not connected to the game server, cannot send a server console command");
                        return;
                    }

                    // execute command on Server-side
                    Instance.CallServer(_ => _.ServerRemote_ExecuteCommand(text));
                    return;
                }
            }

            var commandData = SharedParseCommand(text, out _);
            if (commandData is null)
            {
                Logger.Warning($"Unknown console command: \"{text}\"", byCharacter);
                return;
            }

            if (IsServer)
            {
                Logger.Important(
                    $"Executing console command \"{text}\" from {byCharacter?.ToString() ?? "[system console]"}",
                    byCharacter);
            }

            commandData.Execute(byCharacter);
        }

        private static BaseConsoleCommand SharedFindConsoleCommandByName(string commandName)
        {
            if (string.IsNullOrEmpty(commandName))
            {
                return null;
            }

            BaseConsoleCommand consoleCommand = null;
            foreach (var command in allCommands)
            {
                if (command.GetNameOrAlias(commandName)
                           .Equals(commandName, StringComparison.OrdinalIgnoreCase))
                {
                    consoleCommand = command;
                    break;
                }
            }

            return consoleCommand;
        }

        private static IReadOnlyList<string> SharedSuggestAutocomplete(
            string text,
            ICharacter byCharacter,
            ushort textPosition,
            out ConsoleCommandVariant consoleCommandVariant)
        {
            if (string.IsNullOrEmpty(text))
            {
                consoleCommandVariant = null;
                return EmptySuggestionsArray;
            }

            var commandData = SharedParseCommand(text, out var parsedCommandName, textPosition);
            IReadOnlyList<string> suggestions;

            var isOperator = ServerIsOperatorOrSystemConsole(byCharacter);
            var isModerator = ServerIsModeratorOrSystemConsole(byCharacter);

            if (commandData is null) // don't have parsed command - suggest commands
            {
                if (text.Contains(' ')
                    && text.TrimEnd(' ').Contains(' '))
                {
                    // cannot suggest commands as user entered a space char in between which is supposed to be a command with args
                    consoleCommandVariant = null;
                    return EmptySuggestionsArray;
                }

                var suggestedCommands = SharedGetCommandNamesSuggestions(parsedCommandName ?? string.Empty);
                SharedFilterAvailableCommands(suggestedCommands, isOperator, isModerator);

                suggestions = suggestedCommands.Select(sc => sc.GetNameOrAlias(parsedCommandName)).ToList();
                consoleCommandVariant = suggestedCommands.FirstOrDefault()?.Variants.FirstOrDefault();
            }
            else // parsed command - suggest parameters for this command
            {
                var consoleCommandKind = commandData.ConsoleCommand.Kind;
                if (!isOperator
                    && consoleCommandKind.HasFlag(ConsoleCommandKinds.ServerOperator)
                    && (IsServer
                        || !consoleCommandKind.HasFlag(ConsoleCommandKinds.Client)))
                {
                    // cannot suggest anything to the non-operator player for an operator-only command
                    consoleCommandVariant = null;
                    return EmptySuggestionsArray;
                }

                if (!isModerator
                    && consoleCommandKind.HasFlag(ConsoleCommandKinds.ServerModerator)
                    && (IsServer
                        || !consoleCommandKind.HasFlag(ConsoleCommandKinds.Client)))
                {
                    // cannot suggest anything to the non-moderator player for a moderator-only command
                    consoleCommandVariant = null;
                    return EmptySuggestionsArray;
                }

                suggestions = commandData.GetParameterSuggestions(byCharacter, out consoleCommandVariant);
            }

            if (suggestions?.Count >= MaxSuggestions)
            {
                suggestions = suggestions.Take(MaxSuggestions).ToList();
            }

            return suggestions ?? EmptySuggestionsArray;
        }

        private void ClientRemote_ConsoleCommandError(string message)
        {
            Logger.Warning($"Server console command error:{Environment.NewLine}{message}");

            NotificationSystem.ClientShowNotification(
                "Console command error",
                message.Replace("\n", "[br]"),
                NotificationColor.Bad,
                writeToLog: false);
        }

        private void ClientRemote_ConsoleCommandResult(string commandName, string result)
        {
            Logger.Important($"Server console command result:{Environment.NewLine}{result}");

            NotificationSystem.ClientShowNotification(
                "Console command executed: " + commandName,
                result.Replace("[", "\\[")
                      .Replace("]",  "\\]")
                      .Replace("\n", "[br]"),
                NotificationColor.Neutral,
                writeToLog: false);
        }

        // Cannot use sequential delivery mode here as the size of the list
        // may be above the packet size limit (especially in Editor).
        [RemoteCallSettings(avoidBuffer: true)]
        private void ClientRemote_GetSuggestionsCallback(
            string consoleCommandName,
            byte variantIndex,
            IReadOnlyList<string> suggestions,
            byte requestId)
        {
            var consoleCommand = SharedFindConsoleCommandByName(consoleCommandName);
            var commandVariant = consoleCommand?.Variants[variantIndex];
            ClientSuggestionsCallback(commandVariant, suggestions ?? EmptySuggestionsArray, requestId);
        }

        private void ServerHandleSystemConsoleCommand(string command)
        {
            // try to execute the system console command (with operator access)
            SharedExecuteConsoleCommand(command, byCharacter: null);
        }

        private void ServerRemote_ExecuteCommand(string text)
        {
            SharedExecuteConsoleCommand(text, byCharacter: ServerRemoteContext.Character);
        }

        [RemoteCallSettings(DeliveryMode.ReliableSequenced, timeInterval: 0.1, avoidBuffer: true)]
        private void ServerRemote_GetSuggestions(string text, ushort textPosition, byte requestId)
        {
            var byCharacter = ServerRemoteContext.Character;
            var suggestions = ServerSuggestAutocomplete(
                text,
                byCharacter,
                textPosition,
                out var consoleCommandVariant);

            BaseConsoleCommand consoleCommand = null;
            byte variantIndex = 0;

            if (consoleCommandVariant is not null)
            {
                consoleCommand = consoleCommandVariant.ConsoleCommand;
                var variants = consoleCommand.Variants;
                for (byte index = 0; index < variants.Count; index++)
                {
                    if (consoleCommandVariant == variants[index])
                    {
                        variantIndex = index;
                        break;
                    }
                }
            }

            var consoleCommandName = consoleCommand?.Name;
            this.CallClient(
                byCharacter,
                _ => _.ClientRemote_GetSuggestionsCallback(consoleCommandName,
                                                           variantIndex,
                                                           suggestions,
                                                           requestId));
        }
    }
}