namespace AtomicTorch.CBND.CoreMod.Systems.Console
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Extensions;
    using AtomicTorch.CBND.GameApi.Scripting;

    [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
    public class ConsoleCommandVariant
    {
        public readonly BaseConsoleCommand ConsoleCommand;

        public readonly ScriptingMethodInfo MethodInfo;

        public readonly IReadOnlyList<CommandParameter> Parameters;

        public ConsoleCommandVariant(BaseConsoleCommand consoleCommand, ScriptingMethodInfo methodInfo)
        {
            this.ConsoleCommand = consoleCommand;
            this.MethodInfo = methodInfo;
            this.Parameters = this.MethodInfo.GetParameters()
                                  .Select(p => new CommandParameter(p))
                                  .ToArray();
        }

        public void Execute(ICharacter byCharacter, string[] args)
        {
            var sendNotification = Api.IsServer
                                   && byCharacter != null;
            try
            {
                var parsedArgs = this.ParseArguments(
                    byCharacter,
                    args,
                    throwExceptionOnUnparsedArgument: true,
                    successfullyParsedArgsCount: out _);
                var resultObj = this.MethodInfo.MethodInfo.Invoke(this.ConsoleCommand, parsedArgs);
                if (resultObj != null)
                {
                    var resultStr = resultObj.ToString();
                    Api.Logger.Important(
                        $"Console command \"{this.ConsoleCommand.Name}\" completed: {resultStr}",
                        byCharacter);

                    if (sendNotification
                        && resultStr.Length < 10000
                        // it will break formatting so let's not include such results
                        && !resultStr.Contains('['))
                    {
                        // send notification to this character
                        NotificationSystem.ServerSendNotification(
                            byCharacter,
                            "Command executed: " + this.ConsoleCommand.Name,
                            resultStr.Replace("\n","[br]"),
                            NotificationColor.Neutral);
                    }
                }
            }
            catch (ConsoleCommandWrongArgumentException ex)
            {
                var commandParameter = this.Parameters[ex.Index];
                var message = new StringBuilder(capacity: 100)
                              .Append(ex.IsMissing ? "Argument missing" : "Argument wrong/unknown value provided")
                              .Append(" - argument #")
                              .Append(ex.Index + 1)
                              .Append(' ')
                              .Append(commandParameter.Name).ToString();

                Api.Logger.Warning(message, byCharacter);

                if (sendNotification)
                {
                    // send notification to this character
                    NotificationSystem.ServerSendNotification(
                        byCharacter,
                        "Command cannot be executed: " + this.ConsoleCommand.Name,
                        message,
                        NotificationColor.Bad);
                }
            }
            catch (Exception ex)
            {
                ex = ex.InnerException ?? ex;
                Api.Logger.Warning(ex.Message, byCharacter);

                if (sendNotification)
                {
                    // send notification to this character
                    NotificationSystem.ServerSendNotification(
                        byCharacter,
                        "Command error: " + this.ConsoleCommand.Name,
                        ex.Message,
                        NotificationColor.Bad);
                }
            }
        }

        public int GetMatchRating(ICharacter byCharacter, string[] args)
        {
            if (args.Length > this.Parameters.Count)
            {
                // completely not matches - too many arguments
                return 0;
            }

            this.ParseArguments(
                byCharacter,
                args,
                throwExceptionOnUnparsedArgument: false,
                successfullyParsedArgsCount: out var successfullyParsedArgsCount);

            var matchedArgsCount = successfullyParsedArgsCount;
            return matchedArgsCount;
        }

        public string[] GetParameterSuggestions(string startsWith, byte parameterIndex)
        {
            if (parameterIndex >= this.Parameters.Count)
            {
                // cannot suggest - there are no so many parameters!
                return null;
            }

            var parameter = this.Parameters[parameterIndex];
            var result = parameter.GetSuggestions(startsWith)
                                  .Take(ConsoleCommandsSystem.MaxSuggestions)
                                  .ToArray();

            if (result.Length == 1
                && result[0] == startsWith)
            {
                // no suggestion in that case - as the only suggestion is exactly the same value as provided
                return Array.Empty<string>();
            }

            return result;
        }

        private object[] ParseArguments(
            ICharacter byCharacter,
            string[] args,
            bool throwExceptionOnUnparsedArgument,
            out byte successfullyParsedArgsCount)
        {
            successfullyParsedArgsCount = 0;
            var parsedArgs = new object[this.Parameters.Count];
            var successfullyParsedArgs = new bool[this.Parameters.Count];
            for (var index = 0; index < args.Length; index++)
            {
                var commandParameter = this.Parameters[index];
                if (commandParameter.ParseArgument(args[index], out var parsedArg))
                {
                    successfullyParsedArgsCount++;
                    // successfully parsed value
                    parsedArgs[index] = parsedArg;
                    successfullyParsedArgs[index] = true;
                    continue;
                }

                if (throwExceptionOnUnparsedArgument)
                {
                    throw new ConsoleCommandWrongArgumentException(index, isMissing: false);
                }
            }

            for (var index = args.Length; index < this.Parameters.Count; index++)
            {
                if (successfullyParsedArgs[index])
                {
                    // this arg is successfully parsed
                    continue;
                }

                var commandParameter = this.Parameters[index];
                if (commandParameter.GetDefaultValue(byCharacter, out var value))
                {
                    // default value provided
                    parsedArgs[index] = value;
                    continue;
                }

                if (throwExceptionOnUnparsedArgument)
                {
                    throw new ConsoleCommandWrongArgumentException(index, isMissing: true);
                }
            }

            return parsedArgs;
        }
    }
}