// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Debug
{
    using System.Linq;
    using System.Text;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ConsoleDebugPrintResourceGatheringLeaderboard : BaseConsoleCommand
    {
        public override string Description => "Prints resource gathering leaderboard.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "debug.printResourceGatheringLeaderboard";

        public string Execute()
        {
            var sorted = Api.Server.Characters
                            .EnumerateAllPlayerCharacters(onlyOnline: false,
                                                          exceptSpectators: false)
                            .Select(c =>
                                    {
                                        var statistics = PlayerCharacter.GetPrivateState(c).Statistics;
                                        return (Character: c,
                                                Score: statistics.MineralsMined + statistics.TreesCut);
                                    })
                            .Where(c => c.Score > 1)
                            .OrderByDescending(c => c.Score)
                            .Take(100)
                            .ToList();
            var sb = new StringBuilder();

            sb.AppendLine("Resource gathering leaderboard, top-100:")
              .Append("(format: total score | minerals mined | trees cut | total LP)");

            if (sorted.Count == 0)
            {
                sb.AppendLine()
                  .Append("<the leaderboard is empty>");
                return sb.ToString();
            }

            for (var index = 0; index < sorted.Count; index++)
            {
                var entry = sorted[index];
                sb.AppendLine()
                  .Append("#")
                  .Append(index + 1)
                  .Append(" ");

                var clanTag = FactionSystem.SharedGetClanTag(entry.Character);
                if (!string.IsNullOrEmpty(clanTag))
                {
                    sb.Append("[")
                      .Append(clanTag)
                      .Append("] ");
                }

                var characterPrivateState = PlayerCharacter.GetPrivateState(entry.Character);
                var statistics = characterPrivateState.Statistics;

                sb.Append(entry.Character.Name)
                  .Append(" - ")
                  .Append(entry.Score)
                  .Append(" total, minerals: ")
                  .Append(statistics.MineralsMined)
                  .Append(", trees: ")
                  .Append(statistics.TreesCut)
                  .Append(", LP: ")
                  .Append(characterPrivateState.Technologies.LearningPointsAccumulatedTotal);
            }

            return sb.ToString();
        }
    }
}