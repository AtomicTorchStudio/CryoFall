// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Console
{
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Logging;

    public class ConsoleConsoleLogSeverity : BaseConsoleCommand
    {
        public override string Description => "Gets or sets current log severity.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ClientAndServerOperatorOnly;

        public override string Name => "console.logSeverity";

        public string Execute(LogSeverity? severity = null)
        {
            if (severity.HasValue)
            {
                Api.Shared.LogSeverity = severity.Value;
            }

            return "Current log severity is: " + Api.Shared.LogSeverity;
        }
    }
}