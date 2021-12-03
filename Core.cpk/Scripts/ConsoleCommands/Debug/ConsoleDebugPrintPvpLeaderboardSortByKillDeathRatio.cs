namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Debug
{
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.PvpScoreSystem;

    public class ConsoleDebugPrintPvpLeaderboardSortByKillDeathRatio : BaseConsoleCommand
    {
        public override string Description => "Prints PvP leaderboard (sorted by Kill/Death ratio).";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "debug.printPvpLeaderboardSortByKD";

        public string Execute(int minKills)
        {
            return PvpScoreSystem.ServerGetPvpLeaderboardReport(sortByKillDeathRatio: true, minKills);
        }
    }
}