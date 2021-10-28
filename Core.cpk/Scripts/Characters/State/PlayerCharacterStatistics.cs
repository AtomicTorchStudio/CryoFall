namespace AtomicTorch.CBND.CoreMod.Characters
{
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;

    // Currently the data here is not synchronized to the client
    // and can be browsed by the server operator with console commands.
    public class PlayerCharacterStatistics : BaseNetObject
    {
        public uint Deaths { get; set; }

        public uint PvpKills { get; set; }

        public double PvpScore { get; set; } = 1.0;

        [TempOnly]
        public double ServerPvpScoreNextRecoveryTime { get; set; }
    }
}