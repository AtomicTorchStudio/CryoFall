// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Debug
{
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.PvpScoreSystem;

    public class ConsoleDebugPrintPvpLeaderboard : BaseConsoleCommand
    {
        public override string Description => "Prints PvP leaderboard.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "debug.printPvpLeaderboard";

        public string Execute()
        {
            return PvpScoreSystem.ServerGetPvpLeaderboardReport(sortByKillDeathRatio: false, minKills: 0);
        }
    }
}