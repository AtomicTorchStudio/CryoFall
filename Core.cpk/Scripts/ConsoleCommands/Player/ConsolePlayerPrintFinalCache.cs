namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Player
{
    using System.Linq;
    using System.Text;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsolePlayerPrintFinalCache : BaseConsoleCommand
    {
        public override string Description =>
            @"Prints the current final stats cache of the player character.
              It lists all the applied stats with their sources.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ClientAndServerOperatorOnly;

        public override string Name => "player.printFinalCache";

        public string Execute([CurrentCharacterIfNull] ICharacter player = null)
        {
            var finalStatsCache = player.SharedGetFinalStatsCache();
            var sb = new StringBuilder()
                     .AppendLine()
                     .Append(player)
                     .AppendLine(" final stats cache:");

            foreach (var pair in finalStatsCache.EnumerateFinalValues()
                                                .OrderBy(s => s.Key.ToString()))
            {
                sb.Append("* ")
                  .Append(pair.Key)
                  .Append(": ")
                  .AppendLine(pair.Value.ToString("0.##"));
            }

            sb.AppendLine()
              .AppendLine("Cache sources:");

            foreach (var group in finalStatsCache.Sources.List
                                                 .GroupBy(e => e.StatName)
                                                 .OrderBy(s => s.Key.ToString()))
            {
                var statName = group.Key;

                sb.Append("* ")
                  .Append(statName)
                  .AppendFormat(
                      " (final value: {0:0.###}, mult: {1:0.###})",
                      finalStatsCache[statName],
                      finalStatsCache.GetMultiplier(statName))
                  .AppendLine();

                foreach (var entry in group.OrderBy(e => e.Source.Id))
                {
                    sb.Append("  * ")
                      .Append(entry.Source.GetType().Name)
                      .Append(": ");

                    if (entry.Value != 0)
                    {
                        // append value
                        if (entry.Value > 0)
                        {
                            // add plus sing
                            sb.Append('+');
                        }

                        sb.Append(entry.Value.ToString("0.###"));
                        sb.Append(' ');
                    }

                    if (entry.Percent != 0)
                    {
                        // append value
                        if (entry.Percent > 0)
                        {
                            // add plus sing
                            sb.Append('+');
                        }

                        sb.Append(entry.Percent.ToString("0.###"))
                          .Append('%');
                    }

                    sb.AppendLine();
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}