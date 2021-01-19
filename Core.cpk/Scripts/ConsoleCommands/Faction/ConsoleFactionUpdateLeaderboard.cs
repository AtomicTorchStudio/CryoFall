// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Faction
{
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.FactionLeaderboardSystem;

    public class ConsoleFactionUpdateLeaderboard : BaseConsoleCommand
    {
        public override string Description =>
            "Forces an update of the factions leaderboard.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "faction.updateLeaderboard";

        public string Execute()
        {
            FactionLeaderboardSystem.ServerForceUpdateLeaderboard();
            return "Leaderboard updated";
        }
    }
}