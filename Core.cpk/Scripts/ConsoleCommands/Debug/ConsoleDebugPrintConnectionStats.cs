// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Debug
{
    using System;
    using System.Linq;
    using System.Text;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ConsoleDebugPrintConnectionStats : BaseConsoleCommand
    {
        public override string Description => "Prints network stats for all the connected clients.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "debug.printConnectionStats";

        public string Execute()
        {
            var connections = Api.Server.Core.EnumeratePlayerConnectionStats();
            var sb = new StringBuilder("Active connections:");
            foreach (var stats in connections.OrderBy(c => c.PlayerCharacter.Name))
            {
                sb.AppendLine()
                  .Append("* ")
                  .Append(stats.PlayerCharacter.Name)
                  .Append(" (ID=")
                  .Append(stats.PlayerCharacter.Id)
                  .Append(", IP=")
                  .Append(stats.IpAddress)
                  .Append("): ping game=")
                  .Append(SecondsToMs(stats.LatencyRoundtripGameSeconds))
                  .Append(", ping avg=")
                  .Append(SecondsToMs(stats.LatencyRoundtripAverageSeconds))
                  .Append(", fluctuation=")
                  .Append(SecondsToMs(stats.LatencyFluctuationRangeSeconds))
                  .Append(", jitter=")
                  .Append(SecondsToMs(stats.LatencyJitterSeconds));
            }

            return sb.ToString();
        }

        protected static int SecondsToMs(double seconds)
        {
            return (int)Math.Round(seconds * 1000,
                                   MidpointRounding.AwayFromZero);
        }
    }
}